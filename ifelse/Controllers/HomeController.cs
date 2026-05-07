using System.Diagnostics;
using System.Text.Json;
using ifelse.Data;
using ifelse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ifelse.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(
            AppDbContext context,
            ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var vm = new OrderPageViewModel
            {
                Menus = _context.Menus.ToList(),
                Tables = _context.TablesMeja
                    .OrderBy(x => x.TableNumber)
                    .ToList(),
                Cart = new List<CartItem>()
            };

            vm.LastOrderId =
                HttpContext.Session.GetInt32("LastOrderId");

            // ambil cart dari session
            var cartJson =
                HttpContext.Session.GetString("Cart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                vm.Cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }

            if (vm.LastOrderId != null)
            {
                vm.LastOrder = _context.Orders
                    .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Menu)
                    .FirstOrDefault(x => x.OrderId == vm.LastOrderId.Value);
            }

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                });
        }
    }
}
