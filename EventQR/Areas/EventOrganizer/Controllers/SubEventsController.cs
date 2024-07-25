using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventQR.EF;
using EventQR.Models;
using EventQR.Services;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using EventQR.Models.VM.Reports;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EventQR.Areas.EventOrganizer.Controllers
{
    [Area("EventOrganizer")]
    [Authorize(Roles = "EventOrganizer")]
    public class SubEventsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEventOrganizer _eventService;
        private readonly Organizer _org;
        public SubEventsController(AppDbContext context, IEventOrganizer eventService)
        {
            _context = context;
            _eventService = eventService;
            _org = eventService.GetLoggedInEventOrg();
        }


        // GET: EventOrganizer/SubEvents

        public async Task<IActionResult> Index()
        {

            var thisEvent = _eventService.GetCurrentEvent();
            if (thisEvent != null)
            {
                var sEvent = await _context.SubEvents.Where(ts => ts.EventId == thisEvent.UniqueId).ToListAsync();
                return View(sEvent);
            }
            else
            {

                return RedirectToAction("Index", "SubEvents");
            }
        }

        // GET: EventOrganizer/SubEvents/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subEvent = await _context.SubEvents
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (subEvent == null)
            {
                return NotFound();
            }

            return View(subEvent);
        }

        // GET: EventOrganizer/SubEvents/Create
        public async Task<IActionResult> Create(Guid id)
        {
            SubEvent _subEvent = null;
            var currentEvent = _eventService.GetCurrentEvent();
            if (currentEvent != null)
            {
                _subEvent = await _context.SubEvents.Where(s => s.UniqueId.Equals(id)
                && s.EventId == currentEvent.UniqueId).FirstOrDefaultAsync();

                _subEvent ??= new SubEvent() { StartDateTime = currentEvent.StartDate.Value.AddHours(1), EndDateTime = currentEvent.EndDate.Value.AddHours(-1) };

            }
            else _subEvent ??= new SubEvent() { StartDateTime = DateTime.Now.AddHours(1), EndDateTime = DateTime.Now.AddHours(2), };
            return View(_subEvent);
        }

        // POST: EventOrganizer/SubEvents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubEvent subEvent)
        {
            if (ModelState.IsValid)
            {
                var currentEvent = _eventService.GetCurrentEvent();
                if (currentEvent != null)
                {
                    if (subEvent.UniqueId.Equals(Guid.Empty))
                    {
                        subEvent.UniqueId = Guid.NewGuid();
                        subEvent.EventId = currentEvent.UniqueId;
                        subEvent.CreatedDate = DateTime.Now;
                        _context.Add(subEvent);
                        await _context.SaveChangesAsync();
                    }
                    else if (subEvent.EventId == currentEvent.UniqueId)
                    {
                        var dbSubEvent = await _context.SubEvents.FindAsync(subEvent.UniqueId);
                        if (dbSubEvent != null)
                        {
                            dbSubEvent.LastUpdatedDate = DateTime.Now;
                            dbSubEvent.SubEventName = subEvent.SubEventName;
                            dbSubEvent.StartDateTime = subEvent.StartDateTime;
                            dbSubEvent.EndDateTime = subEvent.EndDateTime;
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(subEvent);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subEvent = await _context.SubEvents
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (subEvent == null)
            {
                return NotFound();
            }

            return View(subEvent);
        }

        // POST: EventOrganizer/SubEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var subEvent = await _context.SubEvents.FindAsync(id);
            if (subEvent != null)
            {
                _context.SubEvents.Remove(subEvent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubEventExists(Guid id)
        {
            return _context.SubEvents.Any(e => e.UniqueId == id);
        }



        public async Task<IActionResult> GetSubEventGuests(Guid id)
        {
            var _subEvent = await _context.SubEvents.FindAsync(id);
            var _guests = await _context.Guests.Where(g => g.EventId == _subEvent.EventId).ToListAsync();
            var _checkInGuests = await _context.CheckIns.Where(c => c.EventId == _subEvent.EventId && c.SubEventId == id).ToListAsync();




            var thisSubEventGuests = _guests.Where(g => g.AllowedSubEventsIdsCommaList != null &&
                    g.AllowedSubEventsIdsCommaList
                     .Split(',')
                     .Select(id => Guid.TryParse(id, out var parsedId) ? parsedId : (Guid?)null)
                     .Contains(id)).ToList();


            var checkinLogs = new List<SubEventGuestCheckInLogs>();
            foreach (var g in thisSubEventGuests)
            {
                checkinLogs.Add(new SubEventGuestCheckInLogs()
                {
                    GuestName = g.Name,
                    GuestId = g.UniqueId,
                    StartDateTime = _subEvent.StartDateTime.Value,
                    EndDateTime = _subEvent.EndDateTime.Value
                });
            }
            foreach (var c in _checkInGuests)
            {
                var g = checkinLogs.Where(g => g.GuestId == c.GuestId).FirstOrDefault();
                g.CheckInTime = c.CheckIn;
            }
            return View(checkinLogs);
        }

        /// <summary>
        /// id = EventId,
        /// We are getting all the subEvents on the bases of receved event id. and passing dropdown list HTML as partial view.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetSubEventsDDLPartial(Guid id)
        {
            try
            {
                var subevents = await _context.SubEvents.Where(s => s.EventId.Equals(id)).ToListAsync();
                return View(subevents);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return View();
        }

    }
}