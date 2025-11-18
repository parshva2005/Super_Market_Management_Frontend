using Microsoft.AspNetCore.Mvc;

namespace Super_Market_Management.Controllers
{
    public class UserLogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
