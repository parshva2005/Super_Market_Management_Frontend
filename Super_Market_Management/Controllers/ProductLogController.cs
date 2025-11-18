using Microsoft.AspNetCore.Mvc;

namespace Super_Market_Management.Controllers
{
    public class ProductLogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
