using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventQR.EF;
using EventQR.Models;
using Microsoft.AspNetCore.Authorization;
using EventQR.Services;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EventQR.Areas.EventOrganizer.Controllers
{

    [Authorize(Roles = "EventOrganizer")]
    [Area("EventOrganizer")]
    public class EventsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEventOrganizer _eventService;
        private readonly Organizer _org;
        public EventsController(AppDbContext context, IEventOrganizer eventService)
        {
            _context = context;
            _eventService = eventService;
            _org = eventService.GetLoggedInEventOrg();
        }

        // GET: EventOrganizer/Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.Where(e => e.EventOrganizerId == _org.UniqueId).ToListAsync();
            return View(events);
        }

        // GET: EventOrganizer/Events/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            var _event = await _eventService.GetEventDetails(id.Value);
            _eventService.SetCurrentEvent(_event); 
            return View(_event);
        }


        // GET: EventOrganizer/Events/Create
        public async Task<IActionResult> Create(Guid id)
        {
            var _event = await _context.Events.FindAsync(id);
            if (_event == null)
                _event = new Event()
                {
                    EventOrganizerId = _org.UniqueId,
                    CreatedDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(8),
                };
            return View(_event);
        }

        // POST: EventOrganizer/Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event _event)
        {
            if (ModelState.IsValid)
            {
                if (_org.UniqueId == _event.EventOrganizerId)
                {

                    if (_event.UniqueId == Guid.Empty)
                    {
                        _event.UniqueId = Guid.NewGuid();
                        _event.CreatedDate = DateTime.Now;
                        _context.Add(_event);
                    }
                    else
                    {
                        _event.LastUpdatedDate = DateTime.Now;
                        _context.Update(_event);
                    }
                    await _context.SaveChangesAsync();
                    _eventService.SetCurrentEvent(_event);
                    if (_event.IsSubEvents)
                        return RedirectToAction(nameof(Index), "SubEvents");
                    else
                        return RedirectToAction(nameof(Index), "EventGuests");
                }
                else
                {
                    ModelState.AddModelError("Invalid User", "Not a valid Event Manager");
                    return View(_event);
                }
            }
            ViewData["EventOrganizerId"] = new SelectList(_context.EventOrganizers, "UniqueId", "OrganizationName", _event.EventOrganizerId);
            return View(_event);
        }


        // GET: EventOrganizer/Events/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var _event = await _context.Events
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (_event == null)
            {
                return NotFound();
            }
            return View(_event);
        }

        // POST: EventOrganizer/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(Guid id)
        {
            return _context.Events.Any(e => e.UniqueId == id);
        }
    }
}
