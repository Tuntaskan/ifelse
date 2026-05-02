using Microsoft.AspNetCore.Mvc;
using ifelse.Data;
using ifelse.Models;

namespace ifelse.Controllers
{
    public class OwnerController : Controller
    {
        private readonly AppDbContext _context;

        public OwnerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int? roleId = HttpContext.Session.GetInt32("roleId");

            if (roleId != 5)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Username = HttpContext.Session.GetString("username");
            var menus = _context.Menus.ToList();
            return View();
        }
    }
}
