using Microsoft.AspNetCore.Mvc;

namespace ifelse.Controllers
{
    public class TablesMejaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
