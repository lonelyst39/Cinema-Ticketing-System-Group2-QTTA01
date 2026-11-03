namespace WebApplication10.Models
{
    public class SeatLine
    {
        public Seat Seat { get; set; } = null!;
        public decimal Price { get; set; }
    }

    public class CheckoutViewModel
    {
        public Showtime Showtime { get; set; } = null!;
        public List<SeatLine> Lines { get; set; } = new();
        public decimal Subtotal { get; set; }
        public DateTime ExpiresAt { get; set; }
        public List<Promotion> Promotions { get; set; } = new();
    }

    public class TopMovie { public string Title { get; set; } = ""; public int Tickets { get; set; } public decimal Revenue { get; set; } }
    public class RecentBooking
    {
        public int BookingId { get; set; }
        public string Customer { get; set; } = "";
        public string Movie { get; set; } = "";
        public int Seats { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = "";
        public DateTime Date { get; set; }
    }
    public class DayRevenue { public DateTime Day { get; set; } public decimal Amount { get; set; } }

    public class AdminDashboardViewModel
    {
        public decimal RevenueToday { get; set; }
        public decimal RevenueTotal { get; set; }
        public int TicketsSold { get; set; }
        public int BookingCount { get; set; }
        public List<TopMovie> TopMovies { get; set; } = new();
        public List<RecentBooking> Recent { get; set; } = new();
        public List<DayRevenue> Last7Days { get; set; } = new();
    }

    public class HomeIndexViewModel
    {
        public List<Movie> Featured { get; set; } = new();   // 4 phim nổi bật cho hero
        public List<Movie> Movies { get; set; } = new();     // phim của trang hiện tại
        public List<string> Genres { get; set; } = new();
        public string Status { get; set; } = "now";          // "now" | "soon"
        public string? Genre { get; set; }
        public string? Q { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int NowCount { get; set; }
        public int SoonCount { get; set; }
        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
        public int FirstItem => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int LastItem => Math.Min(Page * PageSize, TotalItems);
    }
}
