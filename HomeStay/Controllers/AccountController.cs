using HomeStay.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace HomeStay.Controllers
{
    public class AccountController : Controller
    {
        private readonly HomestayContext _context;

        public AccountController(HomestayContext context)
        {
            _context = context;
        }

        private string Sha256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // ================= LOGIN =================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            string hash = Sha256(password);

            var user = _context.Users.FirstOrDefault(u =>
                u.Username == username &&
                u.PasswordHash == hash &&
                u.IsActive);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetInt32("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Username đã tồn tại";
                return View();
            }

            // 1. Tạo User
            var user = new User
            {
                Username = username,
                PasswordHash = Sha256(password),
                Role = 3,              
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges(); 

          
            var profile = new UserProfile
            {
                UserId = user.UserId,
                FullName = username,
                Email = "",
                Phone = "",
                Address = "",
                Gender = null,
                BirthDate = null
                // Avatar = "default.png" (nếu có)
            };

            _context.UserProfiles.Add(profile);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult Profile()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
                return RedirectToAction("Login");

            var profile = _context.UserProfiles
                .FirstOrDefault(p => p.UserId == userId);

            return View(profile);
        }

        [HttpPost]
        public IActionResult Profile(UserProfile model)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
                return RedirectToAction("Login");

            var profile = _context.UserProfiles
                .FirstOrDefault(p => p.UserId == userId);

            profile.FullName = model.FullName;
            profile.Email = model.Email;
            profile.Phone = model.Phone;
            profile.Address = model.Address;
            profile.Gender = model.Gender;
            profile.BirthDate = model.BirthDate;

            _context.SaveChanges();

            ViewBag.Success = "Cập nhật thành công";
            return View(profile);
        }

    }
}
