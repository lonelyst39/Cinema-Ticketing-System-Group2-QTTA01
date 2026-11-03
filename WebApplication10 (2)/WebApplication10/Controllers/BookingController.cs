using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using WebApplication10.Infrastructure;
using WebApplication10.Models;

namespace WebApplication10.Controllers
{
    public class BookingController : Controller
    {
        private const string Locked = "Locked";
        private const string VietQrKey = "vietqr";
        private static readonly Dictionary<string, string> PaymentMethods = new()
        {
            [VietQrKey] = "Thanh toán bằng mã QR"
        };

        private readonly CinemaTicketContext _context;
        private readonly ILogger<BookingController> _logger;
        private readonly IConfiguration _config;

        public BookingController(CinemaTicketContext context, ILogger<BookingController> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        private bool IsAdmin => HttpContext.Session.GetString("UserRole") == "Admin";

        // 1. Chi tiết phim và chọn suất chiếu
        public async Task<IActionResult> SelectShowtime(int movieId, DateTime? selectedDate)
        {
            var movie = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.MovieId == movieId);
            if (movie == null) return NotFound();

            var date = (selectedDate ?? DateTime.Today).Date;
            if (date < DateTime.Today) date = DateTime.Today;
            var nextDate = date.AddDays(1);

            var showtimes = await _context.Showtimes.AsNoTracking()
                .Include(s => s.Room).ThenInclude(r => r.Cinema)
                .Where(s => s.MovieId == movieId && s.StartTime >= date && s.StartTime < nextDate)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            // Số ghế còn trống của từng suất
            var ids = showtimes.Select(s => s.ShowtimeId).ToList();
            var bookedMap = (await _context.BookingDetails
                    .Where(bd => ids.Contains(bd.Booking.ShowtimeId) && bd.Booking.BookingStatus != "Cancelled")
                    .GroupBy(bd => bd.Booking.ShowtimeId)
                    .Select(g => new { ShowtimeId = g.Key, Count = g.Count() })
                    .ToListAsync())
                .ToDictionary(x => x.ShowtimeId, x => x.Count);

            ViewBag.FreeSeats = showtimes.ToDictionary(
                s => s.ShowtimeId,
                s => Math.Max(0, s.Room.Capacity - bookedMap.GetValueOrDefault(s.ShowtimeId)));
            ViewBag.Movie = movie;
            ViewBag.SelectedDate = date;
            return View(showtimes);
        }

        // 2. Sơ đồ ghế
        [SessionAuthorize]
        public async Task<IActionResult> SeatMap(int showtimeId)
        {
            var userId = CurrentUserId;
            var showtime = await _context.Showtimes.AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Room).ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId);
            if (showtime == null) return NotFound();

            if (showtime.StartTime < DateTime.Now)
            {
                TempData["Error"] = "Suất chiếu này đã bắt đầu, vui lòng chọn suất khác.";
                return RedirectToAction(nameof(SelectShowtime), new { movieId = showtime.MovieId });
            }

            var seats = await _context.Seats.AsNoTracking()
                .Where(s => s.RoomId == showtime.RoomId)
                .OrderBy(s => s.RowNumber).ThenBy(s => s.SeatId)
                .ToListAsync();

            var booked = await _context.BookingDetails
                .Where(bd => bd.Booking.ShowtimeId == showtimeId && bd.Booking.BookingStatus != "Cancelled")
                .Select(bd => bd.SeatId).ToListAsync();

            var now = DateTime.Now;
            var locks = await _context.SeatLocks.AsNoTracking()
                .Where(l => l.ShowtimeId == showtimeId && l.LockStatus == Locked && l.ExpiresAt > now)
                .Select(l => new { l.SeatId, l.UserId }).ToListAsync();

            ViewBag.Showtime = showtime;
            ViewBag.Booked = booked.ToHashSet();
            ViewBag.Held = locks.Where(l => l.UserId != userId).Select(l => l.SeatId).ToHashSet();
            ViewBag.Mine = locks.Where(l => l.UserId == userId).Select(l => l.SeatId).ToHashSet();
            return View(seats);
        }

        // 3. Giữ ghế 10 phút rồi chuyển sang thanh toán
        [HttpPost, ValidateAntiForgeryToken, SessionAuthorize]
        public async Task<IActionResult> Hold(int showtimeId, string? selectedSeatIds)
        {
            var userId = CurrentUserId;

            var seatIds = new List<int>();
            foreach (var part in (selectedSeatIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!int.TryParse(part, out var id))
                {
                    TempData["Error"] = "Dữ liệu ghế không hợp lệ!";
                    return RedirectToAction(nameof(SeatMap), new { showtimeId });
                }
                seatIds.Add(id);
            }
            seatIds = seatIds.Distinct().ToList();

            if (seatIds.Count == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một ghế!";
                return RedirectToAction(nameof(SeatMap), new { showtimeId });
            }
            if (seatIds.Count > Pricing.MaxSeats)
            {
                TempData["Error"] = $"Bạn chỉ được đặt tối đa {Pricing.MaxSeats} ghế mỗi lần!";
                return RedirectToAction(nameof(SeatMap), new { showtimeId });
            }

            var showtime = await _context.Showtimes.FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId);
            if (showtime == null) return NotFound();
            if (showtime.StartTime < DateTime.Now)
            {
                TempData["Error"] = "Suất chiếu này đã bắt đầu, không thể đặt vé!";
                return RedirectToAction(nameof(SelectShowtime), new { movieId = showtime.MovieId });
            }

            await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var now = DateTime.Now;

                var validCount = await _context.Seats.CountAsync(s => seatIds.Contains(s.SeatId) && s.RoomId == showtime.RoomId);
                if (validCount != seatIds.Count)
                {
                    TempData["Error"] = "Ghế được chọn không hợp lệ!";
                    return RedirectToAction(nameof(SeatMap), new { showtimeId });
                }

                var sold = await _context.BookingDetails.AnyAsync(bd => bd.Booking.ShowtimeId == showtimeId
                    && seatIds.Contains(bd.SeatId) && bd.Booking.BookingStatus != "Cancelled");
                var heldByOthers = await _context.SeatLocks.AnyAsync(l => l.ShowtimeId == showtimeId && l.UserId != userId
                    && l.LockStatus == Locked && l.ExpiresAt > now && seatIds.Contains(l.SeatId));
                if (sold || heldByOthers)
                {
                    TempData["Error"] = "Một số ghế vừa có người chọn. Vui lòng chọn lại!";
                    return RedirectToAction(nameof(SeatMap), new { showtimeId });
                }

                // Nhả các ghế cũ của chính người này trong suất chiếu, rồi giữ ghế mới
                var mine = await _context.SeatLocks
                    .Where(l => l.ShowtimeId == showtimeId && l.UserId == userId && l.LockStatus == Locked).ToListAsync();
                foreach (var l in mine) l.LockStatus = "Released";

                foreach (var seatId in seatIds)
                {
                    _context.SeatLocks.Add(new SeatLock
                    {
                        ShowtimeId = showtimeId,
                        SeatId = seatId,
                        UserId = userId,
                        LockedAt = now,
                        ExpiresAt = now.AddMinutes(Pricing.HoldMinutes),
                        LockStatus = Locked
                    });
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return RedirectToAction(nameof(Checkout), new { showtimeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi giữ ghế cho suất chiếu {ShowtimeId}", showtimeId);
                TempData["Error"] = "Không thể giữ ghế lúc này, vui lòng thử lại!";
                return RedirectToAction(nameof(SeatMap), new { showtimeId });
            }
        }

        // 4. Trang thanh toán (hiển thị ghế đang được giữ)
        [HttpGet, SessionAuthorize]
        public async Task<IActionResult> Checkout(int showtimeId)
        {
            var locks = await ActiveLocks(showtimeId, CurrentUserId);
            if (locks.Count == 0)
            {
                TempData["Error"] = "Thời gian giữ ghế đã hết. Vui lòng chọn ghế lại!";
                return RedirectToAction(nameof(SeatMap), new { showtimeId });
            }

            var showtime = await _context.Showtimes.AsNoTracking()
                .Include(s => s.Movie).Include(s => s.Room).ThenInclude(r => r.Cinema)
                .FirstAsync(s => s.ShowtimeId == showtimeId);

            var lines = locks.OrderBy(l => l.Seat.RowNumber).ThenBy(l => l.SeatId)
                .Select(l => new SeatLine { Seat = l.Seat, Price = Pricing.SeatPrice(showtime.Price, l.Seat.SeatType) })
                .ToList();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var vm = new CheckoutViewModel
            {
                Showtime = showtime,
                Lines = lines,
                Subtotal = lines.Sum(x => x.Price),
                ExpiresAt = locks.Min(l => l.ExpiresAt),
                Promotions = await _context.Promotions.AsNoTracking()
                    .Where(p => p.Status == "Active" && p.StartDate <= today && p.EndDate >= today && p.Quantity > 0)
                    .ToListAsync()
            };
            return View(vm);
        }

        // Kiểm tra mã khuyến mãi (gọi bằng fetch từ trang thanh toán)
        [HttpPost, ValidateAntiForgeryToken, SessionAuthorize]
        public async Task<IActionResult> ApplyPromo(int showtimeId, string? code)
        {
            var locks = await ActiveLocks(showtimeId, CurrentUserId);
            if (locks.Count == 0) return Json(new { ok = false, message = "Thời gian giữ ghế đã hết." });

            var showtime = await _context.Showtimes.AsNoTracking().FirstAsync(s => s.ShowtimeId == showtimeId);
            var subtotal = locks.Sum(l => Pricing.SeatPrice(showtime.Price, l.Seat.SeatType));

            var promo = await FindPromotion(code);
            if (promo == null)
                return Json(new { ok = false, message = "Mã không hợp lệ, đã hết hạn hoặc hết lượt." });

            var discount = Pricing.Discount(subtotal, promo);
            return Json(new { ok = true, message = "Đã áp dụng mã " + promo.PromotionCode, discount, total = subtotal - discount });
        }

        // 5. Thanh toán và xuất vé
        [HttpPost, ValidateAntiForgeryToken, SessionAuthorize]
        public async Task<IActionResult> Pay(int showtimeId, string? paymentMethod, string? promoCode)
        {
            var userId = CurrentUserId;
            if (paymentMethod == null || !PaymentMethods.TryGetValue(paymentMethod, out var methodName))
            {
                TempData["Error"] = "Vui lòng chọn phương thức thanh toán!";
                return RedirectToAction(nameof(Checkout), new { showtimeId });
            }
            var isQr = paymentMethod == VietQrKey;   // chuyển khoản QR: đơn ở trạng thái Pending, chờ xác nhận

            await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var locks = await _context.SeatLocks.Include(l => l.Seat)
                    .Where(l => l.ShowtimeId == showtimeId && l.UserId == userId && l.LockStatus == Locked && l.ExpiresAt > DateTime.Now)
                    .ToListAsync();
                if (locks.Count == 0)
                {
                    TempData["Error"] = "Thời gian giữ ghế đã hết. Vui lòng chọn ghế lại!";
                    return RedirectToAction(nameof(SeatMap), new { showtimeId });
                }

                var showtime = await _context.Showtimes.FirstAsync(s => s.ShowtimeId == showtimeId);
                if (showtime.StartTime < DateTime.Now)
                {
                    TempData["Error"] = "Suất chiếu này đã bắt đầu!";
                    return RedirectToAction(nameof(SelectShowtime), new { movieId = showtime.MovieId });
                }

                var seatIds = locks.Select(l => l.SeatId).ToList();
                if (await _context.BookingDetails.AnyAsync(bd => bd.Booking.ShowtimeId == showtimeId
                        && seatIds.Contains(bd.SeatId) && bd.Booking.BookingStatus != "Cancelled"))
                {
                    TempData["Error"] = "Một số ghế đã có người đặt. Vui lòng chọn lại!";
                    return RedirectToAction(nameof(SeatMap), new { showtimeId });
                }

                var prices = locks.ToDictionary(l => l.SeatId, l => Pricing.SeatPrice(showtime.Price, l.Seat.SeatType));
                var subtotal = prices.Values.Sum();
                var total = subtotal;
                int? promotionId = null;

                if (!string.IsNullOrWhiteSpace(promoCode))
                {
                    var promo = await FindPromotion(promoCode);
                    if (promo == null)
                    {
                        TempData["Error"] = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn!";
                        return RedirectToAction(nameof(Checkout), new { showtimeId });
                    }
                    total = subtotal - Pricing.Discount(subtotal, promo);
                    promotionId = promo.PromotionId;
                    promo.Quantity -= 1;
                }

                var now = DateTime.Now;
                var booking = new Booking
                {
                    BookingDate = now,
                    TotalAmount = total,
                    BookingStatus = isQr ? "Pending" : "Completed",
                    UserId = userId,
                    ShowtimeId = showtimeId,
                    PromotionId = promotionId
                };

                foreach (var l in locks.OrderBy(l => l.Seat.RowNumber).ThenBy(l => l.SeatId))
                {
                    booking.BookingDetails.Add(new BookingDetail
                    {
                        SeatId = l.SeatId,
                        Price = prices[l.SeatId],
                        Ticket = isQr ? null : NewTicket(now)   // vé QR chỉ phát hành sau khi xác nhận thanh toán
                    });
                    l.LockStatus = "Consumed";
                }

                // Lưu ý: đây là thanh toán mô phỏng. Khi tích hợp cổng thật (MoMo/VNPay),
                // chỉ đặt PaymentStatus = "Success" sau khi nhận callback xác nhận từ cổng.
                booking.Payment = new Payment
                {
                    PaymentMethod = methodName,
                    Amount = total,
                    PaymentDate = now,
                    PaymentStatus = isQr ? "Pending" : "Success",
                    TransactionCode = isQr
                        ? "VE" + Guid.NewGuid().ToString("N")[..8].ToUpper()      // dùng làm nội dung chuyển khoản
                        : "TXN-" + Guid.NewGuid().ToString("N")[..16].ToUpper()
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return isQr
                    ? RedirectToAction(nameof(QrPayment), new { bookingId = booking.BookingId })
                    : RedirectToAction(nameof(Confirmation), new { bookingId = booking.BookingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt vé cho suất chiếu {ShowtimeId}", showtimeId);
                TempData["Error"] = "Có lỗi xảy ra trong quá trình đặt vé, vui lòng thử lại!";
                return RedirectToAction(nameof(SeatMap), new { showtimeId });
            }
        }

        // 5b. Trang quét mã QR chuyển khoản Vietcombank
        [HttpGet, SessionAuthorize]
        public async Task<IActionResult> QrPayment(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null || booking.Payment == null) return NotFound();
            if (booking.UserId != CurrentUserId && !IsAdmin) return StatusCode(StatusCodes.Status403Forbidden);

            if (booking.BookingStatus == "Completed")
                return RedirectToAction(nameof(Confirmation), new { bookingId });

            var expiresAt = booking.BookingDate.AddMinutes(Pricing.HoldMinutes);
            if (booking.BookingStatus != "Pending" || expiresAt <= DateTime.Now)
            {
                if (booking.BookingStatus == "Pending")
                {
                    booking.BookingStatus = "Cancelled";          // nhả ghế
                    booking.Payment.PaymentStatus = "Failed";
                    await _context.SaveChangesAsync();
                }
                TempData["Error"] = "Đơn thanh toán đã hết hạn. Vui lòng chọn ghế lại!";
                return RedirectToAction(nameof(SeatMap), new { showtimeId = booking.ShowtimeId });
            }

            var bin = _config["VietQr:BankBin"] ?? VietQrHelper.VietcombankBin;
            var account = _config["VietQr:AccountNo"] ?? "1050556154";   // mặc định nếu appsettings chưa có
            var payload = VietQrHelper.BuildPayload(bin, account, (long)booking.TotalAmount, booking.Payment.TransactionCode);

            ViewBag.QrImage = GenerateQRCodeBase64(payload);
            ViewBag.BankName = _config["VietQr:BankName"] ?? "Vietcombank";
            ViewBag.AccountNo = account;
            ViewBag.AccountName = _config["VietQr:AccountName"] ?? "AOV Cinemas";
            ViewBag.Content = booking.Payment.TransactionCode;
            ViewBag.ExpiresAt = expiresAt;
            return View(booking);
        }

        // 5c. Xác nhận đã chuyển khoản -> phát hành vé.
        // LƯU Ý: đây là bước mô phỏng. Khi chạy thật, hãy bỏ nút này và chỉ gọi logic bên dưới
        // từ webhook của dịch vụ đối soát (SePay, Casso, payOS...) hoặc Admin xác nhận sau khi kiểm tra sao kê.
        [HttpPost, ValidateAntiForgeryToken, SessionAuthorize]
        public async Task<IActionResult> ConfirmTransfer(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .Include(b => b.BookingDetails).ThenInclude(d => d.Ticket)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null || booking.Payment == null) return NotFound();
            if (booking.UserId != CurrentUserId && !IsAdmin) return StatusCode(StatusCodes.Status403Forbidden);

            if (booking.BookingStatus != "Pending"
                || booking.BookingDate.AddMinutes(Pricing.HoldMinutes) <= DateTime.Now)
            {
                TempData["Error"] = "Đơn thanh toán không còn hiệu lực.";
                return RedirectToAction(nameof(QrPayment), new { bookingId });
            }

            var now = DateTime.Now;
            foreach (var d in booking.BookingDetails.Where(d => d.Ticket == null))
                d.Ticket = NewTicket(now);

            booking.BookingStatus = "Completed";
            booking.Payment.PaymentStatus = "Success";
            booking.Payment.PaymentDate = now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Confirmation), new { bookingId });
        }

        // 6. Vé của tôi
        [SessionAuthorize]
        public async Task<IActionResult> MyTickets()
        {
            var bookings = await _context.Bookings.AsNoTracking()
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Include(b => b.BookingDetails).ThenInclude(d => d.Seat)
                .Where(b => b.UserId == CurrentUserId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
            return View(bookings);
        }

        // 7. Xác nhận / chi tiết vé (chỉ chủ vé hoặc Admin)
        [SessionAuthorize]
        public async Task<IActionResult> Confirmation(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.Seat)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.Ticket)
                .Include(b => b.Payment)
                .AsSplitQuery()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return NotFound();
            if (booking.UserId != CurrentUserId && !IsAdmin) return StatusCode(StatusCodes.Status403Forbidden);

            ViewBag.CanCancel = booking.BookingStatus == "Completed"
                && booking.Showtime.StartTime > DateTime.Now.AddHours(Pricing.CancelBeforeHours)
                && booking.BookingDetails.All(d => d.Ticket?.TicketStatus == "Valid");
            return View(booking);
        }

        // 8. Hủy vé (trước giờ chiếu 2 tiếng), hoàn lại ghế và lượt khuyến mãi
        [HttpPost, ValidateAntiForgeryToken, SessionAuthorize]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Showtime)
                .Include(b => b.BookingDetails).ThenInclude(d => d.Ticket)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null || (booking.UserId != CurrentUserId && !IsAdmin)) return NotFound();

            if (booking.BookingStatus != "Completed"
                || booking.Showtime.StartTime <= DateTime.Now.AddHours(Pricing.CancelBeforeHours)
                || booking.BookingDetails.Any(d => d.Ticket?.TicketStatus != "Valid"))
            {
                TempData["Error"] = $"Chỉ hủy được vé chưa sử dụng và trước giờ chiếu ít nhất {Pricing.CancelBeforeHours} tiếng.";
                return RedirectToAction(nameof(Confirmation), new { bookingId });
            }

            booking.BookingStatus = "Cancelled";
            foreach (var d in booking.BookingDetails)
                if (d.Ticket != null) d.Ticket.TicketStatus = "Cancelled";
            if (booking.Payment != null) booking.Payment.PaymentStatus = "Refunded";

            if (booking.PromotionId != null)
            {
                var promo = await _context.Promotions.FindAsync(booking.PromotionId);
                if (promo != null) promo.Quantity += 1;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã hủy vé. Số tiền sẽ được hoàn về phương thức thanh toán của bạn.";
            return RedirectToAction(nameof(Confirmation), new { bookingId });
        }

        // ----- Hàm dùng chung -----
        private Task<List<SeatLock>> ActiveLocks(int showtimeId, int userId) =>
            _context.SeatLocks.Include(l => l.Seat)
                .Where(l => l.ShowtimeId == showtimeId && l.UserId == userId && l.LockStatus == Locked && l.ExpiresAt > DateTime.Now)
                .ToListAsync();

        private async Task<Promotion?> FindPromotion(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var c = code.Trim();
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionCode == c && p.Status == "Active"
                && p.StartDate <= today && p.EndDate >= today && p.Quantity > 0);
        }

        private static Ticket NewTicket(DateTime now)
        {
            var code = "TK" + now.ToString("yyMMdd") + Guid.NewGuid().ToString("N")[..8].ToUpper();
            return new Ticket
            {
                TicketCode = code,
                Qrcode = GenerateQRCodeBase64(code),
                IssuedDate = now,
                TicketStatus = "Valid"
            };
        }

        private static string GenerateQRCodeBase64(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return "data:image/png;base64," + Convert.ToBase64String(qrCode.GetGraphic(10));
        }
    }
}
