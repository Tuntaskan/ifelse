using Microsoft.AspNetCore.Mvc;

namespace ifelse.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
