using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using ifelse.Data;
using ifelse.Models;
using System.Linq;

namespace ifelse.Controllers
{
    public class MenuController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private static readonly string[] AllowedPhotoExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

        public MenuController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private bool TrySavePhoto(IFormFile? photoFile, out string? fileName, out string? errorMessage)
        {
            fileName = null;
            errorMessage = null;

            if (photoFile == null || photoFile.Length == 0)
                return true;

            var extension = Path.GetExtension(photoFile.FileName);

            if (string.IsNullOrWhiteSpace(extension) || !AllowedPhotoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                errorMessage = "Format foto tidak didukung. Gunakan JPG, PNG, GIF, atau WEBP.";
                return false;
            }

            try
            {
                var imagesPath = Path.Combine(_environment.WebRootPath, "images");
                Directory.CreateDirectory(imagesPath);

                fileName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine(imagesPath, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                photoFile.CopyTo(stream);

                return true;
            }
            catch
            {
                errorMessage = "Foto gagal diupload. Coba ulangi dengan file lain.";
                return false;
            }
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

            if (!TrySavePhoto(menu.PhotoFile, out var fileName, out var errorMessage))
            {
                TempData["MenuError"] = errorMessage;
                return RedirectToAction("Index");
            }

            menu.Photo = fileName;

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

            if (!TrySavePhoto(menu.PhotoFile, out var fileName, out var errorMessage))
            {
                TempData["MenuError"] = errorMessage;
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(fileName))
                existingMenu.Photo = fileName;

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
