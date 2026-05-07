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

        private bool IsAllowed()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            return roleId == 1 || roleId == 3;
        }

        private async Task UpdateTableStatusAsync(int tableId)
        {
            var table = await _context.TablesMeja.FindAsync(tableId);

            if (table == null)
                return;

            var activeOrders = await _context.Orders
                .Where(x =>
                    x.TableId == tableId &&
                    x.OrderStatus != "Done")
                .ToListAsync();

            // kalau tidak ada order aktif
            if (!activeOrders.Any())
            {
                table.Status = "Available";
                return;
            }

            // kalau masih ada order aktif
            table.Status = "Occupied";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var vm = new OrderPageViewModel();

            // kasir/admin lihat semua order + detail receipt
            vm.Orders = await _context.Orders
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Menu)
                .OrderBy(x => x.OrderId)
                .ThenBy(x => x.OrderId)
                .ToListAsync();

            // menu
            vm.Menus = await _context.Menus
                .ToListAsync();

            vm.Tables = await _context.TablesMeja
                .OrderBy(x => x.TableNumber)
                .ToListAsync();

            ViewBag.Tables = vm.Tables;

            // cart session
            var cartJson =
                HttpContext.Session.GetString("Cart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                vm.Cart = JsonSerializer
                    .Deserialize<List<CartItem>>(cartJson)
                    ?? new List<CartItem>();
            }

            vm.LastOrderId =
                HttpContext.Session.GetInt32("LastOrderId");

            if (vm.LastOrderId != null)
            {
                vm.LastOrder = await _context.Orders
                    .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Menu)
                    .FirstOrDefaultAsync(x => x.OrderId == vm.LastOrderId.Value);
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(
            int menuId,
            int qty)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

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

            var currentQty =
                existingItem?.Qty ?? 0;

            var totalQty =
                currentQty + qty;

            if (totalQty > menu.Stock)
            {
                TempData["CartError"] =
                    $"{menu.MenuName} only left {menu.Stock}.";

                return RedirectToAction("Index");
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

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(
            int? customerId,
            string? customerName,
            string? customerRequest,
            int? tableId)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

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

                CustomerName = customerName ?? string.Empty,

                CustomerRequest = customerRequest,

                TableId =
                    tableId == 0 ? null : tableId,

                OrderDate = DateTime.Now,

                PaymentStatus = "Pending",

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

            if (tableId != null && tableId != 0)
            {
                await UpdateTableStatusAsync(tableId.Value);
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
            string status,
            string paymentStatus,
            int? tableId)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var order =
                await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            var oldTableId = order.TableId;

            order.OrderStatus = status;
            order.PaymentStatus = paymentStatus;
            order.TableId = tableId == 0 ? null : tableId;

            await _context.SaveChangesAsync();

            if (oldTableId != null)
            {
                await UpdateTableStatusAsync(oldTableId.Value);
            }

            if (order.TableId != null)
            {
                await UpdateTableStatusAsync(order.TableId.Value);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ResetCheckout()
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("LastOrderId");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(
            int id)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var order = await _context.Orders
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x =>
                    x.OrderId == id);

            if (order == null)
                return NotFound();

            var oldTableId = order.TableId;

            // hapus detail dulu
            _context.OrderDetails.RemoveRange(
                order.OrderDetails);

            // lalu order
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            if (oldTableId != null)
            {
                await UpdateTableStatusAsync(oldTableId.Value);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        // untuk popup receipt kasir
        public async Task<IActionResult> Receipt(
            int id)
        {
            var order = await _context.Orders
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Menu)
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
