using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ifelse.Data;
using ifelse.Models;

namespace ifelse.Controllers
{
    public class SupervisorController : Controller
    {
        private readonly AppDbContext _context;

        public SupervisorController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAllowed()
        {
            return HttpContext.Session.GetInt32("roleId") == 2;
        }

        private ManagementDashboardViewModel BuildDashboardViewModel()
        {
            return new ManagementDashboardViewModel
            {
                Inventory = _context.Inventory
                    .OrderBy(x => x.ItemName)
                    .ToList(),
                BrokenReports = _context.BrokenItemReports
                    .Include(x => x.Inventory)
                    .OrderByDescending(x => x.ReportDate)
                    .ToList()
            };
        }

        public IActionResult Index()
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            ViewBag.Username = HttpContext.Session.GetString("username");
            return View(BuildDashboardViewModel());
        }
    }
}
