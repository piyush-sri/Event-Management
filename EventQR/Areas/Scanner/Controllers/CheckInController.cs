using EventQR.EF;
using EventQR.Models;
using EventQR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EventQR.Areas.Scanner.Controllers
{
    [Area("Scanner")]
    //   [Authorize(Roles = "Scanner")]
    public class CheckInController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEventOrganizer _eventService;

        public CheckInController(AppDbContext context, IEventOrganizer eventService)
        {
            _context = context;
            _eventService = eventService;
        }

        public async Task<IActionResult> AllowGuest(string id)
        {
            var DecryptString = EventQR.Common.Static.Variables.Decrypt(id);
            Guid guestId = Guid.Empty;
            Guid eventId = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(DecryptString))
            {
                var ids = DecryptString.Split(",");
                Guid.TryParse(ids[0], out guestId);
                Guid.TryParse(ids[1], out eventId);
            }
            var guest = await _eventService.GetAllDetailsForGuest(guestId);

            GuestCheckIn _checkin = new GuestCheckIn()
            {
                UserLoginId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                GuestId = guestId,
                Guest = guest,
                EventId = eventId,
                Event = guest.MyEvent,
                CheckIn = DateTime.Now,
            };
            _eventService.SetCurrentEvent(guest.MyEvent);
            return View(_checkin);
        }
        [HttpPost]
        public async Task<IActionResult> AllowGuest(Guid subEventId, Guid eventId, Guid GuestId)
        {
            GuestCheckIn _checkin = new GuestCheckIn()
            {
                UserLoginId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                GuestId = GuestId,
                EventId = eventId,
                SubEventId = subEventId,
                CheckIn = DateTime.Now
            };
            var guest = await _context.Guests
                //.Include(g=>g.MyEvent)
                .Include(g => g.MyEvent.SubEvents)
                .Where(g => g.UniqueId == GuestId && g.EventId == eventId).FirstOrDefaultAsync();
            if (guest != null)
            {
                _checkin.Guest = guest;
                _checkin.Event = guest.MyEvent;
                var r = guest.AllowedSubEventsIdsCommaList.Contains(subEventId.ToString());
                await _context.CheckIns.AddAsync(_checkin);
                await _context.SaveChangesAsync();
            } 
            _checkin = await _eventService.GetGuestCheckInDto(GuestId);
            return View(_checkin);
        }

        public async Task<IActionResult> GuestList()
        {
            var thisEvent = _eventService.GetCurrentEvent();
            if (thisEvent != null)
            {
                var list = await _context.Guests.Where(g => g.EventId == thisEvent.UniqueId).ToListAsync();
                return View(list);
            }
            return View();
            // return View("guest");
        }

        public IActionResult EventDetails()
        {
            var thisEvent = _eventService.GetCurrentEvent();
            return View(thisEvent);
        }
        public IActionResult ScanQr()
        {
            return View();
        }
    }
}
