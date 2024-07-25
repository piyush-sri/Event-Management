using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventQR.EF;
using EventQR.Models;
using Microsoft.AspNetCore.Authorization;

namespace EventQR.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")] 
    [Authorize(Roles = "SuperAdmin")]
    public class EventOrganizersController : Controller
    {
        private readonly AppDbContext _context;

        public EventOrganizersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SuperAdmin/EventOrganizers
        public async Task<IActionResult> Index()
        {
            return View(await _context.EventOrganizers.ToListAsync());
        }

        // GET: SuperAdmin/EventOrganizers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organizer = await _context.EventOrganizers
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (organizer == null)
            {
                return NotFound();
            }

            return View(organizer);
        }

        // GET: SuperAdmin/EventOrganizers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SuperAdmin/EventOrganizers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UniqueId,OrganizerUserId,OrganizationName,Name,Phone1,Phone2,Email,Website,OfficeAddress,CreatedDate,LastUpdatedDate")] Organizer organizer)
        {
            if (ModelState.IsValid)
            {
                organizer.UniqueId = Guid.NewGuid();
                _context.Add(organizer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(organizer);
        }

        // GET: SuperAdmin/EventOrganizers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organizer = await _context.EventOrganizers.FindAsync(id);
            if (organizer == null)
            {
                return NotFound();
            }
            return View(organizer);
        }

        // POST: SuperAdmin/EventOrganizers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("UniqueId,OrganizerUserId,OrganizationName,Name,Phone1,Phone2,Email,Website,OfficeAddress,CreatedDate,LastUpdatedDate")] Organizer organizer)
        {
            if (id != organizer.UniqueId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organizer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizerExists(organizer.UniqueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(organizer);
        }

        // GET: SuperAdmin/EventOrganizers/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organizer = await _context.EventOrganizers
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (organizer == null)
            {
                return NotFound();
            }

            return View(organizer);
        }

        // POST: SuperAdmin/EventOrganizers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var organizer = await _context.EventOrganizers.FindAsync(id);
            if (organizer != null)
            {
                _context.EventOrganizers.Remove(organizer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrganizerExists(Guid id)
        {
            return _context.EventOrganizers.Any(e => e.UniqueId == id);
        }
    }
}
