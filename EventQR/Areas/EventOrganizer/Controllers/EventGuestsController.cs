using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventQR.EF;
using EventQR.Models;
using EventQR.Services;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;


namespace EventQR.Areas.EventOrganizer.Controllers
{
    [Area("EventOrganizer")]
    [Authorize(Roles = "EventOrganizer")]
    public class EventGuestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEventOrganizer _eventService;
        private readonly IQrCodeGenerator _qrService;
        private readonly Organizer _org;
        private readonly Event _thisEvent;
        public EventGuestsController(AppDbContext context, IEventOrganizer eventService, IQrCodeGenerator qrService)
        {
            _context = context;
            _eventService = eventService;
            _org = eventService.GetLoggedInEventOrg();
            _qrService = qrService;
        }

        // GET: EventOrganizer/EventGuests

        public async Task<IActionResult> Index()
        {

            var thisEvent = _eventService.GetCurrentEvent();
            if (thisEvent != null)
            {
                var Event = await _context.Guests.Where(ts => ts.EventId == thisEvent.UniqueId).ToListAsync();
                return View(Event);


            }
            else
            {

                return RedirectToAction("Index", "EventGuests");
            }
        }

        // GET: EventOrganizer/EventGuests/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventGuest = await _context.Guests
                .Include(e => e.MyEvent)
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (eventGuest == null)
            {
                return NotFound();
            }

            return View(eventGuest);
        }

        // GET: EventOrganizer/EventGuests/Create
        public async Task<IActionResult> Create(Guid id)
        {
            EventGuest _guest = null;
            var currentEvent = _eventService.GetCurrentEvent();
            if (currentEvent != null)
            {
                currentEvent.SubEvents = await _context.SubEvents.Where(e => e.EventId == currentEvent.UniqueId).ToListAsync();
                _guest = await _context.Guests.Where(s => s.UniqueId.Equals(id)
                && s.EventId == currentEvent.UniqueId).FirstOrDefaultAsync();
                _guest ??= new EventGuest() { EventId = currentEvent.UniqueId };
                _guest.SubEvents = currentEvent.SubEvents;
                _guest.SubEvents.ForEach(e =>
                {
                    if (!string.IsNullOrWhiteSpace(_guest.AllowedSubEventsIdsCommaList))
                        if (_guest.AllowedSubEventsIdsCommaList.Contains(e.UniqueId.ToString()))
                            e.IsIncludedForThisGuest = true;
                });
            }
            ViewBag.currentEvent = currentEvent;
            return View(_guest);
        }

        // POST: EventOrganizer/EventGuests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventGuest eventGuest)
        {
            if (ModelState.IsValid)
            {
                var currentEvent = _eventService.GetCurrentEvent();


                if (currentEvent != null)
                {
                    List<string> selectedIds = null;
                    if (eventGuest.SubEvents != null)
                        selectedIds = eventGuest?.SubEvents.Where(e => e.IsIncludedForThisGuest && e.UniqueId != null).Select(e => e.UniqueId.ToString()).ToList();
                    if (eventGuest.UniqueId.Equals(Guid.Empty))
                    {
                        eventGuest.UniqueId = Guid.NewGuid();
                        eventGuest.EventId = currentEvent.UniqueId;
                        eventGuest.CreatedDate = DateTime.Now;
                        eventGuest.AllowedSubEventsIdsCommaList = selectedIds == null ? null : string.Join(",", selectedIds);
                        _context.Add(eventGuest);
                    }
                    else if (eventGuest.EventId == currentEvent.UniqueId)
                    {
                        var dbGuest = await _context.Guests.FindAsync(eventGuest.UniqueId);
                        if (dbGuest != null)
                        {
                            dbGuest.Name = eventGuest.Name;
                            dbGuest.MobileNo1 = eventGuest.MobileNo1;
                            dbGuest.MobileNo2 = eventGuest.MobileNo2;
                            dbGuest.Email = eventGuest.Email;
                            dbGuest.AllowedSubEventsIdsCommaList = selectedIds == null ? null : string.Join(",", selectedIds);
                            dbGuest.LastUpdatedDate = DateTime.Now;
                        }
                    }

                    var result = await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eventGuest);
        }

        // GET: EventOrganizer/EventGuests/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventGuest = await _context.Guests
                .Include(e => e.MyEvent)
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (eventGuest == null)
            {
                return NotFound();
            }

            return View(eventGuest);
        }

        // POST: EventOrganizer/EventGuests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var eventGuest = await _context.Guests.FindAsync(id);
            if (eventGuest != null)
            {
                _context.Guests.Remove(eventGuest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventGuestExists(Guid id)
        {
            return _context.Guests.Any(e => e.UniqueId == id);
        }


        [AllowAnonymous]
        public async Task<IActionResult> ShowMyTicket(Guid guestId, Guid eventId)
        {
            // _myQrCode.GenerateQRCode(guest.UniqueId);

            var _guest = await _context.Guests.FindAsync(guestId);
            if (_guest.EventId == eventId)
            {
                _guest.QrCodeImageUri = _qrService.GenerateQRCode(guestId, eventId);
                _guest.MyEvent = await _context.Events.Where(e => e.UniqueId.Equals(_guest.EventId)).FirstOrDefaultAsync();
            }
            return View(_guest.MyEvent.TicketViewName, _guest);
        }


        [AllowAnonymous]
        public async Task<IActionResult> Invitation(Guid guestId, Guid eventId)
        {
            var _guest = await _context.Guests.FindAsync(guestId);
            if (_guest != null)
                if (_guest.EventId == eventId)
                    _guest.MyEvent = await _context.Events.Where(e => e.UniqueId.Equals(_guest.EventId)).FirstOrDefaultAsync();
            return View(_guest);
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Invitation(EventGuest _guest)
        {
            var guestDb = await _context.Guests.FindAsync(_guest.UniqueId);
            if (guestDb != null)
            {
                if (guestDb.EventId == _guest.EventId && guestDb.UniqueId == _guest.UniqueId)
                {
                    guestDb.GuestCount = _guest.GuestCount;
                    guestDb.IsInviteAccepted = _guest.IsInviteAccepted;
                    guestDb.InviteAcceptedOn = System.DateTime.Now;
                }
                _context.Guests.Update(guestDb);
                await _context.SaveChangesAsync();
            }

            return View(_guest);
        }

        public async Task<IActionResult> Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file != null)
            {
                var _thisEvent = _eventService.GetCurrentEvent();
                var list = new List<EventGuest>();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var rowcount = worksheet.Dimension.Rows;
                        for (int row = 2; row <= rowcount; row++)
                        {
                            var guest = new EventGuest();
                            guest.Name = worksheet.Cells[row, 1].Value.ToString().Trim();
                            guest.Email = worksheet.Cells[row, 2].Value.ToString().Trim();
                            guest.MobileNo1 = worksheet.Cells[row, 3].Value.ToString().Trim();
                            guest.MobileNo2 = worksheet.Cells[row, 4].Value.ToString().Trim();
                            guest.EventId = _thisEvent.UniqueId;
                            guest.CreatedDate = DateTime.UtcNow;
                            _context.Guests.Add(guest);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                TempData["GuestListUploadSuccess"] = true;
            }
            return RedirectToAction("Index");
        }

    }
}
