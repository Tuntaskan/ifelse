using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ifelse.Data;
using ifelse.Models;

namespace ifelse.Controllers
{
    public class TablesMejaController : Controller
    {
        private readonly AppDbContext _context;

        public TablesMejaController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAllowed()
        {
            var roleId = HttpContext.Session.GetInt32("roleId");

            return roleId == 1 || roleId == 2;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var tables = await _context.TablesMeja
                .OrderBy(x => x.TableNumber)
                .ToListAsync();

            return View(tables);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TableMeja table)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            if (table.TableNumber <= 0)
            {
                TempData["TableError"] = "Number table must be above 0.";
                return RedirectToAction("Index");
            }

            var tableExists = await _context.TablesMeja
                .AnyAsync(x => x.TableNumber == table.TableNumber);

            if (tableExists)
            {
                TempData["TableError"] = "Table number is curently in use.";
                return RedirectToAction("Index");
            }

            table.Status = string.IsNullOrWhiteSpace(table.Status)
                ? "Available"
                : table.Status;

            _context.TablesMeja.Add(table);
            await _context.SaveChangesAsync();

            TempData["TableSuccess"] = "New table added succeed.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAvailability(int id, string status)
        {
            if (!IsAllowed())
                return RedirectToAction("Index", "Home");

            var table = await _context.TablesMeja.FindAsync(id);

            if (table == null)
                return NotFound();

            table.Status = status;

            await _context.SaveChangesAsync();

            TempData["TableSuccess"] = "Table status available succeed.";

            return RedirectToAction("Index");
        }
    }
}
