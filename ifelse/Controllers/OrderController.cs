using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ifelse.Data;
using ifelse.Models;
using System.Text.Json;

namespace ifelse.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new OrderPageViewModel();

            // kasir/admin lihat semua order + detail receipt
            vm.Orders = await _context.Orders
                .Include(x => x.OrderDetails)
                .OrderByDescending(x => x.OrderId)
                .ToListAsync();

            // menu
            vm.Menus = await _context.Menus
                .ToListAsync();

            // cart session
            var cartJson =
                HttpContext.Session.GetString("Cart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                vm.Cart = JsonSerializer
                    .Deserialize<List<CartItem>>(cartJson)
                    ?? new List<CartItem>();
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(
            int menuId,
            int qty)
        {
            var menu =
                await _context.Menus.FindAsync(menuId);

            if (menu == null)
                return NotFound();

            var cartJson =
                HttpContext.Session.GetString("Cart");

            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonSerializer
                    .Deserialize<List<CartItem>>(cartJson)
                    ?? new List<CartItem>();

            var existingItem = cart
                .FirstOrDefault(x => x.MenuId == menuId);

            if (existingItem != null)
            {
                existingItem.Qty += qty;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MenuId = menu.MenuId,
                    MenuName = menu.MenuName,
                    Price = menu.Price,
                    Qty = qty
                });
            }

            HttpContext.Session.SetString(
                "Cart",
                JsonSerializer.Serialize(cart));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(
            int? customerId,
            int? tableId)
        {
            var cartJson =
                HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return RedirectToAction("Index");
            }

            var cart = JsonSerializer
                .Deserialize<List<CartItem>>(cartJson)
                ?? new List<CartItem>();

            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            // buat order utama
            var order = new Order
            {
                CustomerId =
                    customerId == 0 ? null : customerId,

                TableId =
                    tableId == 0 ? null : tableId,

                OrderDate = DateTime.Now,

                PaymentStatus = "Paid",

                OrderStatus = "Waiting",

                TotalPrice =
                    cart.Sum(x => x.Subtotal)
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            // simpan detail + kurangi stock
            foreach (var item in cart)
            {
                var menu = await _context.Menus
                    .FindAsync(item.MenuId);

                if (menu != null)
                {
                    // kurangi stock
                    menu.Stock -= item.Qty;

                    if (menu.Stock < 0)
                    {
                        menu.Stock = 0;
                    }
                }

                _context.OrderDetails.Add(
                    new OrderDetail
                    {
                        OrderId = order.OrderId,

                        MenuId = item.MenuId,

                        Qty = item.Qty,

                        Price = item.Price,

                        Subtotal = item.Subtotal
                    });
            }

            await _context.SaveChangesAsync();

            // simpan receipt id
            HttpContext.Session.SetInt32(
                "LastOrderId",
                order.OrderId);

            // kosongkan cart
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status)
        {
            var order =
                await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            order.OrderStatus = status;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(
            int id)
        {
            var order = await _context.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x =>
                    x.OrderId == id);

            if (order == null)
                return NotFound();

            // hapus detail dulu
            _context.OrderDetails.RemoveRange(
                order.OrderDetails);

            // lalu order
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // untuk popup receipt kasir
        public async Task<IActionResult> Receipt(
            int id)
        {
            var order = await _context.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x =>
                    x.OrderId == id);

            if (order == null)
                return NotFound();

            return PartialView(
                "_ReceiptPartial",
                order);
        }
    }
}