using EventQR.EF;
using EventQR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EventQR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {

            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        [HttpPost]
       
        public async Task<IActionResult> Contact(Inquery model)
        { 
            if (ModelState.IsValid)
            {
              model.CreatedDate = DateTime.Now;
             await _context.AddAsync(model);
             await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            } 
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
