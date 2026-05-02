using Microsoft.AspNetCore.Mvc;
using ifelse.Data;
using ifelse.Models;

namespace ifelse.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // LIST ORDER
        public IActionResult Index()
        {
            var orders = _context.Orders.ToList();
            return View(orders);
        }

        // CREATE ORDER (GET)
        public IActionResult Create()
        {
            ViewBag.Menu = _context.Menus.ToList();
            return View();
        }

        // CREATE ORDER (POST)
        [HttpPost]
        public async Task<IActionResult> Create(int menuId, int qty)
        {
            var menu = await _context.Menus.FindAsync(menuId);

            if (menu == null)
            {
                return NotFound();
            }

            var subtotal = menu.Price * qty;

            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalPrice = subtotal,
                Status = "Waiting"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var detail = new OrderDetail
            {
                OrderId = order.Id,
                MenuId = menuId,
                Qty = qty,
                Subtotal = subtotal
            };

            _context.OrderDetails.Add(detail);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}