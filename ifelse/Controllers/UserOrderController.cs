using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ifelse.Data;
using ifelse.Models;
using System.Text.Json;

namespace ifelse.Controllers
{
    public class UserOrderController : Controller
    {
        private readonly AppDbContext _context;

        public UserOrderController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // hanya customer/member
            var roleId = HttpContext.Session.GetInt32("roleId");

            if (roleId != 6)
            {
                return RedirectToAction("Index", "Home");
            }

            var vm = new OrderPageViewModel();

            // customer cuma lihat menu, bukan semua order
            vm.Menus = await _context.Menus
                .ToListAsync();

            vm.Tables = await _context.TablesMeja
                .OrderBy(x => x.TableNumber)
                .ToListAsync();

            // ambil cart dari session
            var cartJson =
                HttpContext.Session.GetString("Cart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                vm.Cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
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
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson)
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

            return Redirect(
                Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(
            string customerName,
            string? customerRequest,
            int? tableId)
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            if (roleId != 6)
            {
                return RedirectToAction("Index", "Home");
            }

            var username =
                HttpContext.Session.GetString("username");

            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Username == username);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var cartJson =
                HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return RedirectToAction("Index");
            }

            var cart = string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            // diskon member
            decimal totalPrice =
                cart.Sum(x => x.Subtotal);

            if (user.IsMember == true)
            {
                // contoh diskon 10%
                totalPrice =
                    totalPrice * 0.9m;
            }

            var order = new Order
            {
                CustomerId = user.UserId,
                CustomerName =
                roleId == 6
                    ? user.FullName
                    : customerName,
                CustomerRequest = customerRequest,
                TableId = tableId == 0 ? null : tableId,
                OrderDate = DateTime.Now,
                // nanti kasir yang handle
                PaymentStatus = "Pending",
                // kasir akan lihat ini
                OrderStatus = "Waiting",
                TotalPrice = totalPrice
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            if (tableId != null)
            {
                var selectedTable =
                    await _context.TablesMeja
                        .FindAsync(tableId);

                if (selectedTable != null)
                {
                    selectedTable.Status = "Booked";
                }
            }

            foreach (var item in cart)
            {
                var menu = await _context.Menus
                    .FindAsync(item.MenuId);

                if (menu != null)
                {
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

            HttpContext.Session.SetInt32("LastOrderId", order.OrderId);
            // kosongkan cart
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Receipt", new { id = order.OrderId });
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var order = await _context.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x => x.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

    }
}