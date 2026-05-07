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

        private IActionResult RedirectToCheckoutPage()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            return roleId == 6
                ? RedirectToAction("Index", "Member")
                : RedirectToAction("Index", "Home");
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

            if (qty <= 0)
            {
                TempData["CartError"] =
                    "Amount of order is not valid.";

                return Redirect(
                    Request.Headers["Referer"].ToString());
            }

            var cartJson =
                HttpContext.Session.GetString("Cart");

            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartJson)
                  ?? new List<CartItem>();

            var existingItem = cart
                .FirstOrDefault(x => x.MenuId == menuId);

            int totalQty =
                qty;

            if (totalQty > menu.Stock)
            {
                TempData["CartError"] =
                    $"Stock {menu.MenuName} only left {menu.Stock}.";

                return Redirect(
                    Request.Headers["Referer"].ToString());
            }

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
            int? tableId,
            DateTime? reservationDate)
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            var username =
                HttpContext.Session.GetString("username");

            var user = string.IsNullOrWhiteSpace(username)
                ? null
                : await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.Username == username);

            var isMember = roleId == 6 && user?.IsMember == true;

            if (tableId == 0)
            {
                tableId = null;
            }

            // kalau member booking future date
            if (isMember &&
                reservationDate != null &&
                reservationDate.Value.Date < DateTime.Today)
            {
                TempData["CartError"] =
                    "Reservation date is not valid.";

                return RedirectToCheckoutPage();
            }

            // non-member wajib isi nama
            if (!isMember &&
                string.IsNullOrWhiteSpace(customerName))
            {
                TempData["CartError"] =
                    "Customer name must be fill.";

                return RedirectToAction("Index");
            }

            var cartJson =
                HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return RedirectToAction("Index");
            }

            var cart = string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

            // VALIDASI STOCK ULANG
            foreach (var item in cart)
            {
                var menu = await _context.Menus
                    .FindAsync(item.MenuId);

                if (menu == null)
                {
                    TempData["CartError"] =
                        "Menu is not found.";

                    return Redirect(
                        Request.Headers["Referer"].ToString());
                }

                if (item.Qty > menu.Stock)
                {
                    TempData["CartError"] =
                        $"{menu.MenuName} only left {menu.Stock}.";

                    return Redirect(
                        Request.Headers["Referer"].ToString());
                }
            }

            // diskon member
            decimal totalPrice =
                cart.Sum(x => x.Subtotal);

            if (isMember)
            {
                // contoh diskon 10%
                totalPrice =
                    totalPrice * 0.9m;
            }

            var orderDate = isMember && reservationDate != null
                ? reservationDate.Value
                : DateTime.Now;

            var order = new Order
            {
                CustomerId = user?.UserId,
                CustomerName = isMember
                    ? user!.FullName
                    : customerName,
                CustomerRequest = customerRequest,
                TableId = tableId == 0 ? null : tableId,
                OrderDate = orderDate,
                // nanti kasir yang handle
                PaymentStatus = "Pending",
                // kasir akan lihat ini
                OrderStatus = "Waiting",
                TotalPrice = totalPrice
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            if (isMember && tableId != null)
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
            HttpContext.Session.Remove("Cart");
            TempData["ShowReceipt"] = true;

            return RedirectToCheckoutPage();
        }

        [HttpPost]
        public IActionResult ResetCheckout()
        {
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("LastOrderId");

            return RedirectToCheckoutPage();
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
