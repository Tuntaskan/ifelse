using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ifelse.Data;
using ifelse.Models;

namespace ifelse.Controllers
{
    public class KitchenController : Controller
    {
        private readonly AppDbContext _context;

        public KitchenController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAllowed()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            return roleId == 4;
        }

        // Pesanan masuk ke dapur
        public IActionResult Index()
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var vm = new KitchenDashboardViewModel
            {
                Orders = _context.Orders
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Menu)
                .Where(x => x.PaymentStatus == "Paid")
                .OrderBy(x => x.OrderDate)
                .ToList()
            };

            return View(vm);
        }

        // Update status masak
        [HttpPost]
        public IActionResult UpdateStatus(
            int id,
            string status)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var order = _context.Orders
                .FirstOrDefault(x => x.OrderId == id);

            if (order != null)
            {
                order.OrderStatus = status;

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
