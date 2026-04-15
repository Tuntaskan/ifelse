using Microsoft.AspNetCore.Mvc;
using ifelse.Models;
using Microsoft.AspNetCore.Identity;

namespace ifelse.Controllers
{
    public class LoginController : Controller
    {
        public static List<User> users = new List<User>()
        {
            new User { Username = "admin_ceo", Password = "123" }
        };

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            if (username == "admin_ceo" && password == "123")
            {
                HttpContext.Session.SetString("username", username);
                HttpContext.Session.SetString("role", "admin");
                return RedirectToAction("Index", "Admin");
            }

            ViewBag.Error = "Username atau password salah";
            return View();

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            if (users.Any(u => u.Username == username))
            {
                ViewBag.Error = "username sudah dipakai";
                return View();
            }

            users.Add(new User
            {
                Username = username,
                Password = password
            });

            return RedirectToAction("index");
        }
    }
}

