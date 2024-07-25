using EventQR.EF;
using EventQR.Models;
using EventQR.Models.Acc;
using EventQR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;

namespace EventQR.Areas.EventOrganizer.Controllers
{
    [Authorize(Roles = "EventOrganizer")]
    [Area("EventOrganizer")]
    public class ProfileController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        private readonly IEventOrganizer _eventService;
        private readonly Organizer _org;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(AppDbContext context, IEventOrganizer eventService, SignInManager<AppUser> signInManager, IWebHostEnvironment environment, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _context = context;
            _eventService = eventService;
            _org = eventService.GetLoggedInEventOrg();
            _environment = environment;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(_org);
        }
        [HttpGet]
        public IActionResult MyProfile()
        {
            ViewBag.LogoImageName = _org.LogoImageName;
            return View(_org);
        }
        public IActionResult ChangePassword()
        {

            return View(_org);
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var UserLoginId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            AppUser user = await _userManager.FindByIdAsync(UserLoginId);

            if (user != null)
            {
                // Update login ID
                //       user.UserName = UserLoginId; // Assuming UserName is used for login ID

                // Update password
                var passwordHasher = new PasswordHasher<AppUser>();
                var newPasswordHash = passwordHasher.HashPassword(user, newPassword);
                user.PasswordHash = newPasswordHash;

                // Save changes to the database
                await _context.SaveChangesAsync();
                _context.UpdateRange();
            }
            return View();
        }

        [HttpGet]
        public IActionResult ChangeLogInId()
        {

            return View(_org);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeLogInId(string existingLoginId, string newLoginId)
        {
            // Validate the input
            if (string.IsNullOrWhiteSpace(existingLoginId) || string.IsNullOrWhiteSpace(newLoginId))
            {
                ModelState.AddModelError(string.Empty, "Existing login ID and new login ID cannot be empty.");
                return View(_org);
            }

            // Find the user by existing login ID
            var user = await _userManager.FindByNameAsync(existingLoginId);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User with the existing login ID not found.");
                return View(_org);
            }

            // Update the username (login ID)
            user.UserName = newLoginId;
            user.NormalizedUserName = _userManager.NormalizeName(newLoginId);

            // Save the changes
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Optionally, sign the user out and sign them back in with the new username
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Redirect to a confirmation page or back to the profile page
                return RedirectToAction(nameof(MyProfile));
            }
            else
            {
                // Add errors to the model state to display in the view
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(_org);
        }


        // GET: Organizer/Merchants/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            ViewBag.LogoImageName = _org.LogoImageName;
            return View(_org);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventQR.Models.Organizer orgVm)
        {
            ViewBag.LogoImageName = _org.LogoImageName;
            if (ModelState.IsValid)
            {
                try
                {
                    var orgDb = await _context.EventOrganizers.FindAsync(orgVm.UniqueId);
                    if (orgDb != null)
                    {
                        if (await SaveProfilePic(orgVm))
                            orgDb.ProfileImageName = orgVm.ProfileImageName;
                        if (await SaveLogo(orgVm))
                            orgDb.LogoImageName = orgVm.LogoImageName;
                        orgDb.LastUpdatedDate = DateTime.UtcNow;
                        orgDb.Name = orgVm.Name;
                        orgDb.OrganizationName = orgVm.OrganizationName;
                        orgDb.OfficeAddress = orgVm.OfficeAddress;
                        orgDb.Phone1 = orgVm.Phone1;
                        orgDb.Phone2 = orgVm.Phone2;
                        orgDb.Website = orgVm.Website;

                        var result = await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var msg = ex.Message;
                    if (!EventExists(orgVm.UniqueId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MyProfile));
            }
            //  ViewData["OrganizerUserId"] = new SelectList(_context.Users, "Id", "Id", @event.OrganizerUserId);
            return View(orgVm);
        }

        private bool EventExists(Guid id)
        {
            return (_context.EventOrganizers?.Any(e => e.UniqueId == id)).GetValueOrDefault();
        }
        private async Task<bool> SaveProfilePic(Organizer organizer)
        {
            var path = String.Empty;
            var res = false;
            try
            {
                if (organizer.ProfileImage != null && organizer.ProfileImage.Length > 0)
                {
                    path = _environment.WebRootPath + Common.Static.Variables.OrgProfilePicsPath.Replace("/", "\\");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    organizer.ProfileImageName = organizer.ProfileImage.FileName.Replace(" ", "").Replace("/", "").Replace("-", "").Replace("_", "");
                    await organizer.ProfileImage.CopyToAsync(new FileStream(path + "//" + organizer.ProfileImageName, FileMode.Create));
                    return true;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return res;
        }
        private async Task<bool> SaveLogo(Organizer organizer)
        {
            var restult = false;
            try
            {
                if (organizer.LogoImage != null && organizer.LogoImage.Length > 0)
                {
                    var path = String.Empty;
                    path = _environment.WebRootPath + Common.Static.Variables.OrgLogoPath.Replace("/", "\\");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    organizer.LogoImageName = organizer.LogoImage.FileName.Replace(" ", "").Replace("/", "").Replace("-", "").Replace("_", "");
                    await organizer.LogoImage.CopyToAsync(new FileStream(path + "//" + organizer.LogoImageName, FileMode.Create));
                    return true;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return restult;
        }




        /// <summary>
        /// 
        /// @ToDo , Need extra level of protection to resect password 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        //        [HttpPost]
        //  [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string email, string newPassword)
        {
            AppUser user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // Update login ID
                //  user.UserName = UserLoginId; // Assuming UserName is used for login ID

                // Update password
                var passwordHasher = new PasswordHasher<AppUser>();
                var newPasswordHash = passwordHasher.HashPassword(user, newPassword);
                user.PasswordHash = newPasswordHash;

                // Save changes to the database
                await _context.SaveChangesAsync();
                _context.UpdateRange();
            }
            return View();
        }
    }
}




