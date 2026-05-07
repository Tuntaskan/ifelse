using Microsoft.AspNetCore.Mvc;
using ifelse.Data;
using ifelse.Models;
using Microsoft.EntityFrameworkCore;

namespace ifelse.Controllers
{
    public class OwnerController : Controller
    {
        private readonly AppDbContext _context;

        public OwnerController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAllowed()
        {
            return HttpContext.Session.GetInt32("roleId") == 5;
        }

        private ManagementDashboardViewModel BuildDashboardViewModel(DateTime? salesDate)
        {
            return new ManagementDashboardViewModel
            {
                SalesDate = salesDate,
                Orders = _context.Orders
                    .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Menu)
                    .OrderByDescending(x => x.OrderDate)
                    .ToList(),
                Menus = _context.Menus
                    .OrderBy(x => x.MenuName)
                    .ToList(),
                Inventory = _context.Inventory
                    .OrderBy(x => x.ItemName)
                    .ToList(),
                BrokenReports = _context.BrokenItemReports
                    .Include(x => x.Inventory)
                    .OrderByDescending(x => x.ReportDate)
                    .ToList()
            };
        }

        public IActionResult Index(DateTime? salesDate)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            ViewBag.Username = HttpContext.Session.GetString("username");
            return View(BuildDashboardViewModel(salesDate));
        }
    }
}
