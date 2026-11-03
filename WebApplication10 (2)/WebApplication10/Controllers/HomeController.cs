using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication10.Models;

namespace WebApplication10.Controllers
{
    public class HomeController : Controller
    {
        private readonly CinemaTicketContext _context;

        public HomeController(CinemaTicketContext context)
        {
            _context = context;
        }

        private const int PageSize = 12;

        // Bỏ dấu để tìm "dem" ra "Đêm", giống hành vi tìm kiếm cũ ở phía trình duyệt
        private static string Normalize(string? s)
        {
            var d = (s ?? "").ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(d.Length);
            foreach (var c in d)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            return sb.ToString().Replace('đ', 'd');
        }

        public async Task<IActionResult> Index(string? status, string? genre, string? q, int page = 1)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            status = status == "soon" ? "soon" : "now";
            genre = string.IsNullOrWhiteSpace(genre) ? null : genre;
            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

            var all = _context.Movies.AsNoTracking();

            IQueryable<Movie> query = status == "soon"
                ? all.Where(m => m.ReleaseDate > today).OrderBy(m => m.ReleaseDate).ThenBy(m => m.MovieId)
                : all.Where(m => m.ReleaseDate <= today).OrderByDescending(m => m.ReleaseDate).ThenBy(m => m.MovieId);
            if (genre != null) query = query.Where(m => m.Genre == genre);

            List<Movie> items;
            int total;
            if (q == null)
            {
                // Không tìm kiếm: phân trang ngay trên SQL (Skip/Take)
                total = await query.CountAsync();
                page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));
                items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();
            }
            else
            {
                // Có từ khóa: lọc không phân biệt dấu trong bộ nhớ rồi mới cắt trang
                var key = Normalize(q);
                var matched = (await query.ToListAsync()).Where(m => Normalize(m.Title).Contains(key)).ToList();
                total = matched.Count;
                page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));
                items = matched.Skip((page - 1) * PageSize).Take(PageSize).ToList();
            }

            var vm = new HomeIndexViewModel
            {
                Movies = items,
                Featured = await all.Where(m => m.ReleaseDate <= today)
                    .OrderByDescending(m => m.ReleaseDate).ThenBy(m => m.MovieId).Take(4).ToListAsync(),
                Genres = await all.Select(m => m.Genre).Distinct().OrderBy(g => g).ToListAsync(),
                NowCount = await all.CountAsync(m => m.ReleaseDate <= today),
                SoonCount = await all.CountAsync(m => m.ReleaseDate > today),
                Status = status,
                Genre = genre,
                Q = q,
                Page = page,
                PageSize = PageSize,
                TotalItems = total
            };

            ViewBag.Promotions = await _context.Promotions.AsNoTracking()
                .Where(p => p.Status == "Active" && p.StartDate <= today && p.EndDate >= today && p.Quantity > 0)
                .ToListAsync();
            ViewBag.Cinemas = await _context.Cinemas.AsNoTracking().ToListAsync();

            ViewData["Curtain"] = true; // hiệu ứng mở màn nhung ở trang chủ
            return View(vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code)
        {
            ViewBag.Code = code ?? 500;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
