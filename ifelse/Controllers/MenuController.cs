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

            if (ModelState.IsValid)
            {
                _context.Menus.Add(menu);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(menu);
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

            if (ModelState.IsValid)
            {
                _context.Menus.Update(menu);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(menu);
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