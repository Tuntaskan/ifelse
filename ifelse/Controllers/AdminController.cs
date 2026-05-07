using Microsoft.AspNetCore.Mvc;
using ifelse.Data;
using ifelse.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ifelse.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAllowed()
        {
            return HttpContext.Session.GetInt32("roleId") == 1;
        }

        private ManagementDashboardViewModel BuildDashboardViewModel(DateTime? salesDate)
        {
            return new ManagementDashboardViewModel
            {
                SalesDate = salesDate,

                Orders = _context.Orders
                    .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Menu)
                    .OrderByDescending(x => x.OrderDate)
                    .ToList(),

                Menus = _context.Menus
                    .OrderBy(x => x.MenuName)
                    .ToList(),

                Inventory = _context.Inventory
                    .OrderBy(x => x.ItemName)
                    .ToList(),

                BrokenReports = _context.BrokenItemReports
                    .Include(x => x.Inventory)
                    .OrderByDescending(x => x.ReportDate)
                    .ToList()
            };
        }

        public IActionResult Index(DateTime? salesDate)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            ViewBag.Username = HttpContext.Session.GetString("username");

            return View(BuildDashboardViewModel(salesDate));
        }


        // ==========================
        // CREATE USER PAGE
        // ==========================
        public IActionResult CreateUser()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            ViewBag.Users = _context.Users
                .OrderBy(x => x.FullName)
                .ToList();

            return View(new UserCreateViewModel());
        }


        // ==========================
        // CREATE USER POST
        // ==========================
        [HttpPost]
        public IActionResult CreateUser(UserCreateViewModel model)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                ViewBag.Users = _context.Users
                    .OrderBy(x => x.FullName)
                    .ToList();

                return View(model);
            }

            bool usernameExists = _context.Users
                .Any(x => x.Username == model.Username);

            if (usernameExists)
            {
                ViewBag.Error = "Username already exists.";

                ViewBag.Users = _context.Users
                    .OrderBy(x => x.FullName)
                    .ToList();

                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Username = model.Username,
                PasswordHash = HashPassword(model.Password),
                Phone = model.Phone,
                Email = model.Email,
                RoleId = model.RoleId,
                IsMember = model.IsMember,
                CreatedAt = DateTime.Now,
                Status = "Active"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "User created successfully.";

            return RedirectToAction("CreateUser");
        }


        // ==========================
        // EDIT USER PARTIAL
        // ==========================
        public IActionResult EditUser(int id)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var user = _context.Users.Find(id);

            if (user == null)
                return NotFound();

            return PartialView("_EditUserPartial", user);
        }


        // ==========================
        // EDIT USER POST
        // ==========================
        [HttpPost]
        public IActionResult EditUser(User model)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var user = _context.Users.Find(model.UserId);

            if (user == null)
                return NotFound();

            user.FullName = model.FullName;
            user.Username = model.Username;
            user.PasswordHash = model.PasswordHash;
            user.Phone = model.Phone;
            user.Email = model.Email;
            user.RoleId = model.RoleId;
            user.Status = model.Status;

            _context.SaveChanges();

            TempData["Success"] = "User updated successfully.";

            return RedirectToAction("CreateUser");
        }


        // ==========================
        // DELETE USER PARTIAL
        // ==========================
        public IActionResult DeleteUser(int id)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var user = _context.Users.Find(id);

            if (user == null)
                return NotFound();

            return PartialView("_DeleteUserPartial", user);
        }


        // ==========================
        // SOFT DELETE
        // ==========================
        [HttpPost]
        public IActionResult ConfirmDeleteUser(int id)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var user = _context.Users.Find(id);

            if (user == null)
                return NotFound();

            user.Status = "Inactive";

            _context.SaveChanges();

            TempData["Success"] = "User disabled successfully.";

            return RedirectToAction("CreateUser");
        }


        // ==========================
        // HASH PASSWORD
        // ==========================
        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(
                    Encoding.UTF8.GetBytes(password));

                return Convert.ToBase64String(bytes);
            }
        }
    }
}