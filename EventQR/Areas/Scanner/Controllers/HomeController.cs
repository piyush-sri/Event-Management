using EventQR.EF;
using EventQR.Models;
using EventQR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventQR.Areas.Scanner.Controllers
{
    [Area("Scanner")]
    [Authorize(Roles = "Scanner")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEventOrganizer _eventService;
        private readonly IQrCodeGenerator _qrService;
        private readonly Organizer _org;
        private readonly Event _thisEvent;
        public HomeController(AppDbContext context, IEventOrganizer eventService, IQrCodeGenerator qrService)
        {
            _context = context;
            _eventService = eventService;
            _org = eventService.GetLoggedInEventOrg();
            _qrService = qrService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
        public async Task<IActionResult> Dashboard()
        {
            var _event = _eventService.GetCurrentEvent();
            _event.SubEvents = await _context.SubEvents.Where(e => e.EventId == _event.UniqueId).ToListAsync();
            _event.Guests = await _context.Guests.Where(e => e.EventId == _event.UniqueId).ToListAsync();
            return View(_event);
        }
    }
}
