using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventQR.EF;
using EventQR.Models;
using EventQR.Models.Acc;
using EventQR.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Xml;
using System.Text;

namespace EventQR.Areas.EventOrganizer.Controllers
{
    [Area("EventOrganizer")]
    [Authorize(Roles = "EventOrganizer")]
    public class TicketScannersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEventOrganizer _eventService;
        private readonly IQrCodeGenerator _qrService;
        private readonly Organizer _org;

        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public TicketScannersController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext context, IEventOrganizer eventService, IQrCodeGenerator qrService)
        {
            _context = context;
            _eventService = eventService;
            _org = eventService.GetLoggedInEventOrg();
            _qrService = qrService;
            _userManager = userManager;
            _signInManager = signInManager;

        }
        public async Task<IdentityResult> CreateLoginAccount(AppUser appUser)
        {

            var result = await _userManager.CreateAsync(appUser, appUser.Password);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(appUser, "Scanner").ConfigureAwait(false);
            return result;
        }

        // GET: EventOrganizer/TicketScanners
        public async Task<IActionResult> Index()
        {
            var thisEvent = _eventService.GetCurrentEvent();
            if (thisEvent == null)
            {
                return RedirectToAction("Index", "Events");

            }
            else
            {
                var ticketScanners = await _context.TicketScanners.Where(ts=> ts.EventId == thisEvent.UniqueId).ToListAsync();
                return View(ticketScanners);

            }
          
        }

        // GET: EventOrganizer/TicketScanners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var ticketScanner = await _context.TicketScanners
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (ticketScanner == null)
                return NotFound();

            return View(ticketScanner);
        }
        public async Task<IActionResult> Create(int id)
        {

            var scanner = await _context.TicketScanners.Where(s => s.UniqueId == id
            && s.EventId == _eventService.GetCurrentEvent().UniqueId).FirstOrDefaultAsync();
            if (scanner == null)
                scanner = new TicketScanner()
                {
                    EventId = _eventService.GetCurrentEvent().UniqueId
                };

            return View(scanner);
        }

        // POST: EventOrganizer/TicketScanners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketScanner _scanner)
        {
            if (ModelState.IsValid)
            {
                if (_scanner.UniqueId.Equals(0))
                {
                    _scanner.CreatedData = DateTime.UtcNow;
                    _scanner.EventId = _eventService.GetCurrentEvent().UniqueId;
                    _context.Add(_scanner);
                    var _loginUser = new AppUser()
                    {
                        UserName = _scanner.EmailId,
                        Password = _scanner.Password,
                        ConfirmPassword = _scanner.ConfirmPassword,
                        PhoneNumber = _scanner.Mobile1,
                    };
                    var result = await CreateLoginAccount(_loginUser);
                    if (result.Succeeded)
                        _scanner.UserLoginId = Guid.Parse(_loginUser.Id);
                    else
                    {
                        StringBuilder errorSb = new StringBuilder();
                        foreach (var error in result.Errors)
                            errorSb.Append(error.Code).Append(": ").Append(error.Description).Append("\n");

                        ModelState.AddModelError("Login User Error: ", errorSb.ToString());
                        return View(_scanner);
                    }
                }
                else
                {
                    // var _dbScanner = await _context.FindAsync<TicketScanner>(_scanner.UniqueId);
                    var _dbScanner = await _context.TicketScanners.FindAsync(_scanner.UniqueId);
                    if (_dbScanner != null && _scanner.EventId == _eventService.GetCurrentEvent().UniqueId)
                    {
                        _dbScanner.Name = _scanner.Name;
                        _dbScanner.Mobile1 = _scanner.Mobile1;
                        _dbScanner.Mobile2 = _scanner.Mobile2;
                        _dbScanner.Address = _scanner.Address;
                        _dbScanner.LastUpdatedData = DateTime.UtcNow;
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(_scanner);
        }


        // GET: EventOrganizer/TicketScanners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketScanner = await _context.TicketScanners
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.UniqueId == id);
            if (ticketScanner == null)
            {
                return NotFound();
            }

            return View(ticketScanner);
        }

        // POST: EventOrganizer/TicketScanners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketScanner = await _context.TicketScanners.FindAsync(id);
            if (ticketScanner != null)
            {
                _context.TicketScanners.Remove(ticketScanner);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketScannerExists(int id)
        {
            return _context.TicketScanners.Any(e => e.UniqueId == id);
        }
    }
}
