using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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
        public IActionResult Index()
        {
            var movies = _context.Movies.OrderByDescending(m => m.ReleaseDate).ToList();
            // Lấy danh sách thể loại duy nhất để làm nút lọc Isotope
            ViewBag.Genres = movies.Select(m => m.Genre).Distinct().ToList();
            return View(movies);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
