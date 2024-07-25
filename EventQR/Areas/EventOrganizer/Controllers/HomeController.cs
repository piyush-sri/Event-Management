using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventQR.Areas.EventOrganizer.Controllers
{
    [Area("EventOrganizer")]
    [Authorize(Roles = "EventOrganizer")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
