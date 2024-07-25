using EventQR.EF;
using EventQR.Models;
using EventQR.Models.Reports;
using EventQR.Services;
using EventQR.ViewModels.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data.Common;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Net;
using IronPdf.Engines.Chrome;
using Microsoft.Extensions.Logging;
using System.Xml;

namespace EventQR.Areas.EventOrganizer.Controllers
{
    [Area("EventOrganizer")]
    [Authorize(Roles = "EventOrganizer")]
    public class ReportController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly IEventOrganizer _eventService;
        private readonly Organizer _org;

        public ReportController(AppDbContext context, IEventOrganizer eventService, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _eventService = eventService;
            _httpClient = httpClientFactory.CreateClient("EventQrClient");
            _org = eventService.GetLoggedInEventOrg();
        }
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.Where(e => e.EventOrganizerId == _org.UniqueId).ToListAsync();
            return View(events);
        }
        public async Task<IActionResult> EventReport(Guid eventId)
        {
            if (eventId == Guid.Empty)
            {
                var thisEvent = _eventService.GetCurrentEvent();
                eventId = thisEvent.UniqueId;
            }
            var report = await GetReportData(eventId);
            report.EventId = eventId;
            return View(report);
        }



        [AllowAnonymous]
        public async Task<IActionResult> EventReportHtml(Guid eventId)
        {
            if (Guid.Empty.Equals(eventId))
            {
                return null;
            }
            else
            {
                var report = await GetReportData(eventId);
                return View(report);
            }
        }

        private async Task<EventReportVM> GetReportData(Guid eventId)
        {

            if (eventId.Equals(Guid.Empty))
            {
                var thisEvent = _eventService.GetCurrentEvent();
                eventId = thisEvent.UniqueId;
            }
            EventReportVM eventReportVM = new EventReportVM()
            {
                SubEvents = new List<SubEventVM>(),
                Guests = new List<GuestVM>()
            };

            var dbSubEvent = await _context.SubEvents.Where(s => s.EventId == eventId).ToListAsync();
            var dbCheckIn = await _context.CheckIns.Where(s => s.EventId == eventId).ToListAsync();
            foreach (var s in dbSubEvent)
                eventReportVM.SubEvents.Add(new SubEventVM()
                {
                    SubEventName = s.SubEventName,
                    SubEventId = s.UniqueId,
                    Start = s.StartDateTime.Value,
                    End = s.EndDateTime.Value,
                });

            var dbGuests = await _context.Guests.Where(ts => ts.EventId == eventId).ToListAsync();

            foreach (var g in dbGuests)
            {
                var vmGuest = new GuestVM()
                {
                    GuestId = g.UniqueId,
                    Name = g.Name,
                    allowedSubEventsIdsCommaList = g.AllowedSubEventsIdsCommaList,
                    MySubEvents = new List<SubEventVM>() { },
                    dbCheckIn = dbCheckIn.Where(ts => ts.GuestId == g.UniqueId).ToList()
                };
                if (!string.IsNullOrWhiteSpace(g.AllowedSubEventsIdsCommaList))
                {
                    var sbEvents = dbSubEvent.Where(e => g.AllowedSubEventsIdsCommaList.Split(',').Select(Guid.Parse).Contains(e.UniqueId)).ToList();
                    foreach (var se in sbEvents)
                        vmGuest.MySubEvents.Add(new SubEventVM()
                        {
                            SubEventName = se.SubEventName,
                            SubEventId = se.UniqueId,
                            End = se.EndDateTime.Value,
                            Start = se.StartDateTime.Value,
                        });
                }
                eventReportVM.Guests.Add(vmGuest);
            }
            return eventReportVM;
        }

        [HttpPost]
        public IActionResult TicketShow(string ticketName)
        {
            string imageUrl = string.Empty;

            if (ticketName == "ShowMyTicket")
            {
                imageUrl = Url.Content("~/eventqrimages/tickets/t1.png");
            }
            else if (ticketName == "ShowMyTicket1")
            {
                imageUrl = Url.Content("~/eventqrimages/tickets/t2.png");
            }
            else if (ticketName == "ShowMyTicket2")
            {
                imageUrl = Url.Content("~/eventqrimages/tickets/t3.png");
            }
            else if (ticketName == "ShowMyTicket3")
            {
                imageUrl = Url.Content("~/eventqrimages/tickets/t4.png");
            }
            else if (ticketName == "ShowMyTicket4")
            {
                imageUrl = Url.Content("~/eventqrimages/tickets/t.png");
            }
            else
            {
                imageUrl = Url.Content("");
            }

            return Json(new { success = true, message = "Ticket processed successfully!", ticketName, imageUrl });
        }
        public async Task<IActionResult> PdfReport(Guid eventId)
        {
            try
            {
                ChromePdfRenderer renderer = GetChromePdfRendesrer();

                var response = await _httpClient.GetAsync($"/EventOrganizer/Report/EventReportHtml?eventId={eventId}");
                if (response.IsSuccessStatusCode)
                {
                    var htmlFromMvc = await response.Content.ReadAsStringAsync();
                    using PdfDocument PdfRenderer = renderer.RenderHtmlAsPdf(htmlFromMvc);
                    return File(PdfRenderer.BinaryData, "application/pdf", $"{DateTime.Now}.pdf");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    var notFoundContent = await response.Content.ReadAsStringAsync();
                    return BadRequest(notFoundContent);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }
            return NotFound("Some error occurred while processing the request.");
        }

        private ChromePdfRenderer GetChromePdfRendesrer()
        {
            ChromePdfRenderer renderer = new ChromePdfRenderer();
            renderer.RenderingOptions.SetCustomPaperSizeInInches(12.5, 20);
            renderer.RenderingOptions.PrintHtmlBackgrounds = true;
            renderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Landscape;
            renderer.RenderingOptions.EnableJavaScript = true;
            renderer.RenderingOptions.WaitFor.RenderDelay(50); // in milliseconds
            renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Screen;
            renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.Zoom;
            renderer.RenderingOptions.Zoom = 100;
            renderer.RenderingOptions.CreatePdfFormsFromHtml = true;

            renderer.RenderingOptions.MarginTop = 40; //millimeters
            renderer.RenderingOptions.MarginLeft = 20; //millimeters
            renderer.RenderingOptions.MarginRight = 20; //millimeters
            renderer.RenderingOptions.MarginBottom = 40; //millimeters

            // Can set FirstPageNumber if you have a cover page
            renderer.RenderingOptions.FirstPageNumber = 1; // use 2 if a cover page will be appended
            return renderer;
        }
    }
}
