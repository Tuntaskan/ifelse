using Microsoft.AspNetCore.Mvc;

namespace ifelse.Controllers
{
    public class SupervisorController : Controller
    {
        public IActionResult Index()
        {
            int? roleId = HttpContext.Session.GetInt32("roleId");

            if (roleId != 2)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Username = HttpContext.Session.GetString("username");
            return View();
        }
    }
}
