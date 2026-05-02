using Microsoft.AspNetCore.Mvc;
using ifelse.Data;
using ifelse.Models;
using ifelse.Models;

namespace ifelse.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            int? roleId = HttpContext.Session.GetInt32("roleId");

            if (roleId != 1)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Username = HttpContext.Session.GetString("username");
            return View();
        }
    }
}