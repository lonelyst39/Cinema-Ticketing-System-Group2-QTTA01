using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication10.Infrastructure;
using WebApplication10.Models;

namespace WebApplication10.Controllers
{
    [SessionAuthorize(Role = "Admin")]
    public class AdminController : Controller
    {
        private readonly CinemaTicketContext _context;

        public AdminController(CinemaTicketContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var valid = _context.Bookings.AsNoTracking().Where(b => b.BookingStatus != "Cancelled");

            var vm = new AdminDashboardViewModel
            {
                RevenueToday = await valid.Where(b => b.BookingDate >= today).SumAsync(b => (decimal?)b.TotalAmount) ?? 0,
                RevenueTotal = await valid.SumAsync(b => (decimal?)b.TotalAmount) ?? 0,
                BookingCount = await valid.CountAsync(),
                TicketsSold = await _context.BookingDetails.CountAsync(d => d.Booking.BookingStatus != "Cancelled")
            };

            vm.TopMovies = await _context.BookingDetails.AsNoTracking()
                .Where(d => d.Booking.BookingStatus != "Cancelled")
                .GroupBy(d => d.Booking.Showtime.Movie.Title)
                .Select(g => new TopMovie { Title = g.Key, Tickets = g.Count(), Revenue = g.Sum(d => d.Price) })
                .OrderByDescending(x => x.Tickets).Take(5).ToListAsync();

            vm.Recent = await _context.Bookings.AsNoTracking()
                .OrderByDescending(b => b.BookingDate).Take(10)
                .Select(b => new RecentBooking
                {
                    BookingId = b.BookingId,
                    Customer = b.User.FullName,
                    Movie = b.Showtime.Movie.Title,
                    Seats = b.BookingDetails.Count,
                    Total = b.TotalAmount,
                    Status = b.BookingStatus,
                    Date = b.BookingDate
                }).ToListAsync();

            var from = today.AddDays(-6);
            var raw = await valid.Where(b => b.BookingDate >= from)
                .Select(b => new { b.BookingDate, b.TotalAmount }).ToListAsync();
            vm.Last7Days = Enumerable.Range(0, 7).Select(i => from.AddDays(i))
                .Select(d => new DayRevenue { Day = d, Amount = raw.Where(r => r.BookingDate.Date == d).Sum(r => r.TotalAmount) })
                .ToList();

            return View(vm);
        }

        // Soát vé: nhập mã vé (hoặc quét QR ra mã) để đánh dấu đã sử dụng
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string? code)
        {
            code = code?.Trim();
            if (string.IsNullOrEmpty(code))
            {
                TempData["Error"] = "Vui lòng nhập mã vé.";
                return RedirectToAction(nameof(Dashboard));
            }

            var detail = await _context.BookingDetails
                .Include(d => d.Ticket).Include(d => d.Seat)
                .Include(d => d.Booking).ThenInclude(b => b.Showtime).ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(d => d.Ticket != null && d.Ticket.TicketCode == code);

            if (detail?.Ticket == null)
            {
                TempData["Error"] = "Không tìm thấy vé có mã " + code + ".";
            }
            else if (detail.Ticket.TicketStatus != "Valid")
            {
                TempData["Error"] = "Vé " + code + " không thể sử dụng (trạng thái: " + detail.Ticket.TicketStatus + ").";
            }
            else if (detail.Booking.Showtime.StartTime.Date != DateTime.Today)
            {
                TempData["Error"] = "Vé này dành cho suất ngày " + detail.Booking.Showtime.StartTime.ToString("dd/MM/yyyy") + ", chưa thể vào rạp hôm nay.";
            }
            else
            {
                detail.Ticket.TicketStatus = "Used";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Soát vé thành công: ghế " + detail.Seat.SeatNumber + ", phim " + detail.Booking.Showtime.Movie.Title + ".";
            }
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
