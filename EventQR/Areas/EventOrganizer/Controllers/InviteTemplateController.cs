using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventQR.Areas.EventOrganizer.Controllers
{
    [Authorize(Roles = "EventOrganizer")]
    [Area("EventOrganizer")]
    public class InviteTemplateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
