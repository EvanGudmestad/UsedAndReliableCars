using Microsoft.AspNetCore.Mvc;

namespace UsedAndReliableCars.Controllers
{
    public class SnowiCoded : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
