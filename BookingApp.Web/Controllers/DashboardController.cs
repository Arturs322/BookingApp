using Microsoft.AspNetCore.Mvc;

namespace BookingApp.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
