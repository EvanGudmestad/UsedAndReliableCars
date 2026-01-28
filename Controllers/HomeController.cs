using Microsoft.AspNetCore.Mvc;

namespace UsedAndReliableCars.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
