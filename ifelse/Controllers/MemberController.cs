using Microsoft.AspNetCore.Mvc;
using ifelse.Data;
using ifelse.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

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
                Menus = _context.Menus.ToList(),
                Tables = _context.TablesMeja
                    .OrderBy(x => x.TableNumber)
                    .ToList(),
                LastOrderId = HttpContext.Session.GetInt32("LastOrderId")
            };

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

        // PROFILE PAGE
        public IActionResult Profile()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            if (roleId != 6)
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = HttpContext.Session.GetInt32("userId");

            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = _context.Users.FirstOrDefault(x => x.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            var vm = new MemberProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Username = user.Username,
                Phone = user.Phone,
                Email = user.Email
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(MemberProfileViewModel vm)
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            if (roleId != 6)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = _context.Users.FirstOrDefault(x => x.UserId == vm.UserId);

            if (user == null)
            {
                return NotFound();
            }

            // optional: cek username duplicate
            bool usernameUsed = _context.Users.Any(x =>
                x.UserId != vm.UserId &&
                x.Username == vm.Username);

            if (usernameUsed)
            {
                TempData["ProfileError"] = "Username already in used.";
                return View(vm);
            }

            user.Username = vm.Username;
            user.Phone = vm.Phone;

            _context.SaveChanges();

            TempData["ProfileSuccess"] = "Profile updated succeed.";

            return RedirectToAction("Profile");
        }
    }
}
