using ifelse.Data;
using ifelse.Models;
using Microsoft.AspNetCore.Mvc;

namespace ifelse.Controllers
{
    public class BrokenItemController : Controller
    {
        private readonly AppDbContext _context;

        public BrokenItemController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAllowed()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            return roleId == 1 || roleId == 2;
        }


        // CREATE
        [HttpPost]
        public IActionResult Create(
            BrokenItemReport model,
            string redirectController,
            string redirectAction)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var inventory = _context.Inventory
                .FirstOrDefault(x => x.InventoryId == model.InventoryId);

            if (inventory == null)
            {
                TempData["BrokenItemError"] =
                    "Item not found.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            // optional validation
            if (model.QtyBroken > inventory.Stock)
            {
                TempData["BrokenItemError"] =
                    "Qty more than stock.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            // kurangi stock inventory
            inventory.Stock -= model.QtyBroken;

            _context.BrokenItemReports.Add(model);

            _context.SaveChanges();

            TempData["BrokenItemSuccess"] =
                "Broken item report add succeed.";

            return RedirectToAction(
                redirectAction,
                redirectController
            );
        }


        // EDIT
        [HttpPost]
        public IActionResult Edit(
            BrokenItemReport model,
            string redirectController,
            string redirectAction)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var report = _context.BrokenItemReports
                .FirstOrDefault(x => x.ReportId == model.ReportId);

            if (report == null)
            {
                TempData["BrokenItemError"] =
                    "Report not found.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            report.QtyBroken = model.QtyBroken;
            report.BrokenBy = model.BrokenBy;
            report.ReporterRole = model.ReporterRole;
            report.Notes = model.Notes;

            _context.SaveChanges();

            TempData["BrokenItemSuccess"] =
                "Broken item report update succeed.";

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

            var report = _context.BrokenItemReports
                .FirstOrDefault(x => x.ReportId == id);

            if (report == null)
            {
                TempData["BrokenItemError"] =
                    "Report not found.";

                return RedirectToAction(
                    redirectAction,
                    redirectController
                );
            }

            _context.BrokenItemReports.Remove(report);

            _context.SaveChanges();

            TempData["BrokenItemSuccess"] =
                "Broken item report deleted succeed.";

            return RedirectToAction(
                redirectAction,
                redirectController
            );
        }
    }
}