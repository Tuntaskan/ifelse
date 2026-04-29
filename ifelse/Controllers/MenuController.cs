using Microsoft.AspNetCore.Mvc;
using ifelse.Models;
using ifelse.Data;

namespace ifelse.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        // READ
        public IActionResult Index()
        {
            var menus = _context.Menu.ToList();
            return View(menus);
        }

        // CREATE GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CREATE POST
        [HttpPost]
        public async Task<IActionResult> Create(MenuModel menu)
        {
            if (ModelState.IsValid)
            {
                _context.Menu.Add(menu);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(menu);
        }

        // EDIT GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var menu = await _context.Menu.FindAsync(id);

            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        // EDIT POST
        [HttpPost]
        public async Task<IActionResult> Edit(int id, MenuModel menu)
        {
            var existingMenu = await _context.Menu.FindAsync(id);

            if (existingMenu == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingMenu.NamaMenu = menu.NamaMenu;
                existingMenu.Harga = menu.Harga;
                existingMenu.Kategori = menu.Kategori;
                existingMenu.Deskripsi = menu.Deskripsi;
                existingMenu.IsAvailable = menu.IsAvailable;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(menu);
        }

        // DELETE GET
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var menu = await _context.Menu.FindAsync(id);

            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        // DELETE POST
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menu = await _context.Menu.FindAsync(id);

            if (menu == null)
            {
                return NotFound();
            }

            _context.Menu.Remove(menu);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}