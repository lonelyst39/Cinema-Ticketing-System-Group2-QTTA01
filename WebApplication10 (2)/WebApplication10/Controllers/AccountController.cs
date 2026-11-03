using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication10.Models;

namespace WebApplication10.Controllers
{
    public class AccountController : Controller
    {
        private readonly CinemaTicketContext _context;
        private readonly PasswordHasher<User> _hasher = new();

        public AccountController(CinemaTicketContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập email và mật khẩu!";
                return View();
            }

            var email = username.Trim();
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null ||
                _hasher.VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
                return View();
            }

            HttpContext.Session.Clear(); // tránh dùng lại phiên cũ sau khi đăng nhập
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role.RoleName);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .First();
                return View();
            }

            var email = model.Email.Trim();
            var phone = model.Phone.Trim();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email này đã được đăng ký. Vui lòng dùng email khác!";
                return View();
            }
            if (await _context.Users.AnyAsync(u => u.Phone == phone))
            {
                ViewBag.Error = "Số điện thoại này đã được đăng ký!";
                return View();
            }

            var customerRole = await _context.Roles.FirstAsync(r => r.RoleName == "Customer");

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = email,
                Phone = phone,
                Address = model.Address.Trim(),
                RoleId = customerRole.RoleId,
                Password = ""
            };
            user.Password = _hasher.HashPassword(user, model.Password);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ViewBag.Error = "Không thể đăng ký lúc này, vui lòng thử lại!";
                return View();
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}