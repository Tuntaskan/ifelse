using Microsoft.AspNetCore.Mvc;
using ifelse.Models;
using ifelse.Data;
using System.Linq;

namespace ifelse.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u =>
                    u.Username == username &&
                    u.PasswordHash == password);

            if (user == null)
            {
                TempData["LoginError"] = "Username atau password salah";
                return RedirectToAction("Index", "Home");
            }

            string roleName = user.RoleId switch
            {
                1 => "admin",
                2 => "supervisor",
                3 => "kasir",
                4 => "kitchen",
                5 => "owner",
                6 => "customer",
                _ => "unknown"
            };

            HttpContext.Session.SetString("username", user.Username);
            HttpContext.Session.SetInt32("roleId", user.RoleId);
            HttpContext.Session.SetString("role", roleName);

            switch (user.RoleId)
            {
                case 1:
                    return RedirectToAction("Index", "Admin");
                case 2:
                    return RedirectToAction("Index", "Supervisor");
                case 3:
                    return RedirectToAction("Index", "Kasir");
                case 4:
                    return RedirectToAction("Index", "Kitchen");
                case 5:
                    return RedirectToAction("Index", "Owner");
                case 6:
                    return RedirectToAction("Index", "Member");
                default:
                    ViewBag.Error = "Role tidak dikenali";
                    return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Register(
            string fullName,
            string username,
            string password,
            string phone,
            string email)
        {
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Username == username);

            if (existingUser != null)
            {
                TempData["RegisterError"] = "Username sudah dipakai";
                return RedirectToAction("Index", "Home");
            }

            var newUser = new User
            {
                FullName = fullName,
                Username = username,
                PasswordHash = password, // nanti bisa di-hash
                Phone = phone,
                Email = email,

                // otomatis customer
                RoleId = 6,

                // otomatis member
                IsMember = true,

                CreatedAt = DateTime.Now,

                Status = "Active"
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            TempData["Success"] = "Registrasi berhasil. Silakan login.";

            return RedirectToAction("Index", "Member");
        }
    }
}