using Microsoft.AspNetCore.Mvc;
using ifelse.Data;
using ifelse.Models;
using System.Linq;

namespace ifelse.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAllowed()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            return roleId == 1 || roleId == 5;
        }

        // READ
        public IActionResult Index()
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var menus = _context.Menus.ToList();

            return View(menus);
        }

        // CREATE FORM
        public IActionResult Create()
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            return View();
        }

        // CREATE SAVE
        [HttpPost]
        public IActionResult Create(MenuModel menu)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            if (menu.PhotoFile != null)
            {
                string fileName = Guid.NewGuid().ToString()
                                  + Path.GetExtension(menu.PhotoFile.FileName);

                string path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images",
                    fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    menu.PhotoFile.CopyTo(stream);
                }

                menu.Photo = fileName;
            }

            _context.Menus.Add(menu);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // EDIT FORM
        public IActionResult Edit(int id)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var menu = _context.Menus.Find(id);

            if (menu == null)
                return NotFound();

            return View(menu);
        }

        // EDIT SAVE
        [HttpPost]
        public IActionResult Edit(MenuModel menu)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var existingMenu = _context.Menus.Find(menu.MenuId);

            if (existingMenu == null)
                return NotFound();

            existingMenu.MenuName = menu.MenuName;
            existingMenu.CategoryId = menu.CategoryId;
            existingMenu.Price = menu.Price;
            existingMenu.Stock = menu.Stock;

            if (menu.PhotoFile != null)
            {
                string fileName = Guid.NewGuid().ToString()
                                  + Path.GetExtension(menu.PhotoFile.FileName);

                string path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images",
                    fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    menu.PhotoFile.CopyTo(stream);
                }

                existingMenu.Photo = fileName;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // DELETE
        public IActionResult Delete(int id)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var menu = _context.Menus.Find(id);

            if (menu == null)
                return NotFound();

            _context.Menus.Remove(menu);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}