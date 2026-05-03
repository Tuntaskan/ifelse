using Microsoft.AspNetCore.Mvc;
using ifelse.Data;
using ifelse.Models;
using System.Linq;
using System.Text.Json;

namespace ifelse.Controllers
{
    public class MemberController : Controller
    {
        private readonly AppDbContext _context;

        public MemberController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            if (roleId != 6)
            {
                return RedirectToAction("Index", "Home");
            }

            var vm = new OrderPageViewModel
            {
                Menus = _context.Menus.ToList()
            };

            var cartJson =
                HttpContext.Session.GetString("Cart");

            if (!string.IsNullOrEmpty(cartJson))
            {
                vm.Cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }

            return View(vm);
        }
    }
}