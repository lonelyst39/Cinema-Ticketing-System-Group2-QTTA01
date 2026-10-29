using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using WebApplication10.Models;
using WebApplication10.Models;
using WebApplication10.Models;

public class Booking : Controller
{
    private readonly CinemaTicketContext _context;

    public Booking(CinemaTicketContext context)
    {
        _context = context;
    }

    // 1. Trang chi tiết phim & Chọn suất chiếu
    public async Task<IActionResult> SelectShowtime(int movieId, DateTime? selectedDate)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null) return NotFound();

        // Mặc định lấy ngày đầu tiên có trong DB của bạn (01/01/2026) nếu chưa chọn ngày
        var date = selectedDate ?? new DateTime(2026, 1, 1);

        var showtimes = await _context.Showtimes
            .Include(s => s.Room)
                .ThenInclude(r => r.Cinema)
            .Where(s => s.MovieId == movieId && s.StartTime.Date == date.Date)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        ViewBag.Movie = movie;
        ViewBag.SelectedDate = date;
        return View(showtimes);
    }

    // 2. Trang hiển thị Sơ đồ ghế (Seat Map) của suất chiếu
    public async Task<IActionResult> SeatMap(int showtimeId)
    {
        var showtime = await _context.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Room)
                .ThenInclude(r => r.Cinema)
            .FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId);

        if (showtime == null) return NotFound();

        // Lấy toàn bộ 50 ghế của phòng chiếu đó
        var seats = await _context.Seats
            .Where(s => s.RoomId == showtime.RoomId)
            .OrderBy(s => s.RowNumber)
            .ThenBy(s => s.SeatId)
            .ToListAsync();

        // Lấy danh sách ID các ghế đã bán thành công trong suất chiếu này
        var bookedSeatIds = await _context.BookingDetails
            .Include(bd => bd.Booking)
            .Where(bd => bd.Booking.ShowtimeId == showtimeId && bd.Booking.BookingStatus != "Cancelled")
            .Select(bd => bd.SeatId)
            .ToListAsync();

        // Lấy danh sách ID các ghế đang bị khóa tạm thời (SEAT_LOCK chưa hết hạn)
        var lockedSeatIds = await _context.SeatLocks
            .Where(sl => sl.ShowtimeId == showtimeId && sl.LockStatus == "Locked" && sl.ExpiresAt > DateTime.Now)
            .Select(sl => sl.SeatId)
            .ToListAsync();

        ViewBag.Showtime = showtime;
        ViewBag.OccupiedSeats = bookedSeatIds.Union(lockedSeatIds).ToHashSet();

        return View(seats);
    }
    [HttpPost]
    public async Task<IActionResult> Checkout(int showtimeId, string selectedSeatIds, string promoCode)
    {
        if (string.IsNullOrEmpty(selectedSeatIds))
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một ghế!";
            return RedirectToAction("SeatMap", new { showtimeId });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var seatIdList = selectedSeatIds.Split(',').Select(int.Parse).ToList();
            var showtime = await _context.Showtimes.Include(s => s.Room).FirstOrDefaultAsync(s => s.ShowtimeId == showtimeId);
            if (showtime == null) return NotFound();

            // 1. Kiểm tra xem ghế đã bị ai đặt hoặc khóa chưa
            var conflictSeats = await _context.BookingDetails
                .Include(bd => bd.Booking)
                .Where(bd => bd.Booking.ShowtimeId == showtimeId && seatIdList.Contains(bd.SeatId) && bd.Booking.BookingStatus != "Cancelled")
                .AnyAsync();

            if (conflictSeats)
            {
                TempData["Error"] = "Một số ghế bạn chọn vừa có người đặt thành công. Vui lòng chọn lại!";
                return RedirectToAction("SeatMap", new { showtimeId });
            }

            // 2. Tính tiền vé ban đầu (Giả sử: Standard = giá gốc, VIP +15k, Couple +30k)
            decimal totalAmount = 0;
            var seatPrices = new Dictionary<int, decimal>();
            var seats = await _context.Seats.Where(s => seatIdList.Contains(s.SeatId)).ToListAsync();

            foreach (var seat in seats)
            {
                decimal price = showtime.Price;
                if (seat.SeatType == "VIP") price += 15000;
                if (seat.SeatType == "Couple") price += 30000;

                seatPrices[seat.SeatId] = price;
                totalAmount += price;
            }

            // 3. Xử lý mã khuyến mãi (Promotion) nếu có
            int? promotionId = null;
            if (!string.IsNullOrEmpty(promoCode))
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var promotion = await _context.Promotions
                    .FirstOrDefaultAsync(p => p.PromotionCode == promoCode && p.Status == "Active" && p.StartDate <= today && p.EndDate >= today && p.Quantity > 0);

                if (promotion != null)
                {
                    promotionId = promotion.PromotionId;
                    if (promotion.DiscountType == "Percentage")
                    {
                        totalAmount -= totalAmount * (promotion.DiscountValue / 100);
                    }
                    else // Fixed Amount
                    {
                        totalAmount -= promotion.DiscountValue;
                    }
                    if (totalAmount < 0) totalAmount = 0;

                    // Trừ số lượng mã khuyến mãi
                    promotion.Quantity -= 1;
                }
            }

            // 4. Tạo bản ghi BOOKING (Vì DB không có IDENTITY nên phải tự tính Max ID)
            int nextBookingId = (_context.Bookings.Max(b => (int?)b.BookingId) ?? 0) + 1;
            // Tạm gán UserID = 1 (hoặc lấy từ Session của user đang đăng nhập)
            int userId = 1;

            var booking = new WebApplication10.Models.Booking
            {
                BookingId = nextBookingId,
                BookingDate = DateTime.Now,
                TotalAmount = totalAmount,
                BookingStatus = "Completed",
                UserId = userId,
                ShowtimeId = showtimeId,
                PromotionId = promotionId
            };
            _context.Bookings.Add(booking);

            // 5. Tạo BOOKING_DETAIL và TICKET cho từng ghế
            int nextDetailId = (_context.BookingDetails.Max(bd => (int?)bd.BookingDetailId) ?? 0) + 1;
            int nextTicketId = (_context.Tickets.Max(t => (int?)t.TicketId) ?? 0) + 1;
            int nextPaymentId = (_context.Payments.Max(p => (int?)p.PaymentId) ?? 0) + 1;

            foreach (var seatId in seatIdList)
            {
                var detail = new BookingDetail
                {
                    BookingDetailId = nextDetailId++,
                    BookingId = nextBookingId,
                    SeatId = seatId,
                    Price = seatPrices[seatId]
                };
                _context.BookingDetails.Add(detail);

                // Sinh mã vé và QR Code
                string ticketCode = $"CGV-{DateTime.Now:yyyyMMdd}-{nextBookingId}-{seatId}";
                string qrCodeBase64 = GenerateQRCodeBase64(ticketCode);

                var ticket = new Ticket
                {
                    TicketId = nextTicketId++,
                    TicketCode = ticketCode,
                    Qrcode = qrCodeBase64,
                    IssuedDate = DateTime.Now,
                    TicketStatus = "Valid",
                    BookingDetailId = detail.BookingDetailId
                };
                _context.Tickets.Add(ticket);
            }

            // 6. Tạo PAYMENT (Thanh toán giả lập thành công)
            var payment = new Payment
            {
                PaymentId = nextPaymentId,
                PaymentMethod = "VNPAY / MOMO",
                Amount = totalAmount,
                PaymentDate = DateTime.Now,
                PaymentStatus = "Success",
                TransactionCode = $"TXN-{DateTime.Now.Ticks}",
                BookingId = nextBookingId
            };
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction("Confirmation", new { bookingId = nextBookingId });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Có lỗi xảy ra trong quá trình đặt vé: " + ex.Message;
            return RedirectToAction("SeatMap", new { showtimeId });
        }
    }

    // Hàm phụ trợ sinh mã QR dạng chuỗi Base64 để hiển thị lên thẻ vé HTML
    private string GenerateQRCodeBase64(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);
        return "data:image/png;base64," + Convert.ToBase64String(qrCodeBytes);
    }

    // 3. Trang Xác Nhận Đặt Vé Thành Công (Hiển thị thông tin vé & mã QR)
    public async Task<IActionResult> Confirmation(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Room)
                    .ThenInclude(r => r.Cinema)
            .Include(b => b.BookingDetails)
                .ThenInclude(bd => bd.Seat)
            .Include(b => b.BookingDetails)
                .ThenInclude(bd => bd.Ticket)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null) return NotFound();

        return View(booking);
    }
}