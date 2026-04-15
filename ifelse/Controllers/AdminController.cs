using Microsoft.AspNetCore.Mvc;

namespace ifelse.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("role") != "admin" )
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Username = HttpContext.Session.GetString("username");
            return View();
        }
    }
}
