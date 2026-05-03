using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ifelse.Data;

namespace ifelse.Controllers
{
    public class KitchenController : Controller
    {
        private readonly AppDbContext _context;

        public KitchenController(AppDbContext context)
        {
            _context = context;
        }

        // Pesanan masuk ke dapur
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Where(x => x.OrderStatus != "Done")
                .ToList();

            return View(orders);
        }

        // Update status masak
        public IActionResult UpdateStatus(
            int id,
            string status)
        {
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