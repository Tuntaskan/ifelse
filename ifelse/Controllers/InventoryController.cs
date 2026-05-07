using ifelse.Data;
using ifelse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ifelse.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAllowed()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            return roleId == 1 || roleId == 2;
        }

        private bool IsValidCategory(string category)
        {
            var allowedCategories = new List<string>
            {
                "Raw Item",
                "Breakable Item"
            };

            return allowedCategories.Contains(category);
        }

        private bool IsValidUnit(string unit)
        {
            var allowedUnits = new List<string>
            {
                "Kg",
                "Liter",
                "Pcs",
                "Pack"
            };

            return allowedUnits.Contains(unit);
        }


        // CREATE
        [HttpPost]
        public IActionResult Create(
            Inventory model,
            string redirectController,
            string redirectAction)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            if (!IsValidCategory(model.Category))
            {
                TempData["InventoryError"] =
                    "Category not valid.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            if (!IsValidUnit(model.Unit))
            {
                TempData["InventoryError"] =
                    "Unit not valid.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            if (!ModelState.IsValid)
            {
                TempData["InventoryError"] =
                    "inventory data is not valid.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            _context.Inventory.Add(model);

            _context.SaveChanges();

            TempData["InventorySuccess"] =
                "Inventory added succeed.";

            return RedirectToAction(
                redirectAction,
                redirectController
            );
        }


        // EDIT
        [HttpPost]
        public IActionResult Edit(
            Inventory model,
            string redirectController,
            string redirectAction)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var existingItem = _context.Inventory
                .FirstOrDefault(x => x.InventoryId == model.InventoryId);

            if (existingItem == null)
            {
                TempData["InventoryError"] =
                    "Inventory not found.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            

            existingItem.ItemName = model.ItemName;
            existingItem.Category = model.Category;
            existingItem.Stock = model.Stock;
            existingItem.Unit = model.Unit;
            existingItem.Condition = model.Condition;

            _context.SaveChanges();

            TempData["InventorySuccess"] =
                "Inventory updated succeed.";

            return RedirectToAction(
                redirectAction,
                redirectController
            );
        }


        // DELETE
        [HttpPost]
        public IActionResult Delete(
            int id,
            string redirectController,
            string redirectAction)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var item = _context.Inventory
                .Include(x => x.BrokenReports)
                .FirstOrDefault(x => x.InventoryId == id);

            if (item == null)
            {
                TempData["InventoryError"] =
                    "Inventory not found.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            // jangan hapus kalau masih dipakai report
            if (item.BrokenReports.Any())
            {
                TempData["InventoryError"] =
                    "Inventory cannot be deleted because there's broken item report.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            _context.Inventory.Remove(item);

            _context.SaveChanges();

            TempData["InventorySuccess"] =
                "Inventory deleted succeed.";

            return RedirectToAction(
                redirectAction,
                redirectController
            );
        }
    }
}