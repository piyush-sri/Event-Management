using EventQR.EF;
using EventQR.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace EventQR.Services
{
    public class EventOrganizer : IEventOrganizer
    {
        private readonly AppDbContext _dbContext;
        private readonly ITempDataDictionary _tempData;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

        public EventOrganizer(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
            _tempData = _tempDataDictionaryFactory.GetTempData(_httpContextAccessor.HttpContext);
        }

        public async Task<Event> GetEventById(Guid _eventId)
        {
            return await _dbContext.Events.FindAsync(_eventId);
        }

        public Organizer GetLoggedInEventOrg()
        {
            var loggedInEventOrg = _httpContextAccessor
                 .HttpContext.User?.FindFirst("organizer")?.Value;
            if (loggedInEventOrg == null)
                return null;
            else return JsonConvert.DeserializeObject<Organizer>(loggedInEventOrg);
        }

        //-------------------------------------------------------------------------------
        public void SetCurrentEvent(Event _event)
        {
            var settings = new JsonSerializerSettings
            {
                //  ContractResolver = new CustomContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var _json = JsonConvert.SerializeObject(_event, settings);
            _httpContextAccessor.HttpContext.Session.SetString("thisEvent", _json);
        }

        public Event GetCurrentEvent()
        {
            string _thisEventJsonStr = _httpContextAccessor.HttpContext.Session.GetString("thisEvent");
            if (!string.IsNullOrWhiteSpace(_thisEventJsonStr))
            {
                var currentEvent = JsonConvert.DeserializeObject<Event>(_thisEventJsonStr.ToString());
                if (currentEvent != null)
                    return currentEvent;
            }
            return null;
        }




        public Organizer GetLoggedInEventOrgSession()
        {
            string _thisEventJsonStr = _httpContextAccessor.HttpContext.Session.GetString("loggedInEventOrganizer");
            if (!string.IsNullOrWhiteSpace(_thisEventJsonStr))
            {
                var currentEvent = JsonConvert.DeserializeObject<Organizer>(_thisEventJsonStr.ToString());
                if (currentEvent != null)
                    return currentEvent;
            }
            return null;
        }

        public void SetLoggedInEventOrgSession(Organizer _org)
        {
            _httpContextAccessor.HttpContext.Session.SetString("loggedInEventOrganizer", JsonConvert.SerializeObject(_org));
        }

        public async Task<EventGuest> GetAllDetailsForGuest(Guid guestId)
        {
            var guest = await _dbContext.Guests.Include(g => g.MyEvent.SubEvents)
                   .Where(g => g.UniqueId == guestId)
                   .FirstOrDefaultAsync();

            if (guest != null)
            {
                if (!string.IsNullOrWhiteSpace(guest.AllowedSubEventsIdsCommaList))
                {
                    var allowedSubEvents = guest.AllowedSubEventsIdsCommaList.Split(',').Select(Guid.Parse);
                    guest.SubEvents = await _dbContext.SubEvents.Where(e => allowedSubEvents.Contains(e.UniqueId)).ToListAsync();
                    guest.CheckInDetails = await _dbContext.CheckIns.Where(c => c.GuestId == guestId).ToListAsync();
                }
            }
            return guest;
        }

        public async Task<GuestCheckIn> GetGuestCheckInDto(Guid guestId)
        {
            var guest = await GetAllDetailsForGuest(guestId);

            GuestCheckIn _checkin = new GuestCheckIn()
            {
                UserLoginId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                GuestId = guestId,
                Guest = guest,
                EventId = guest.EventId,
                Event = guest.MyEvent,
                CheckIn = DateTime.Now,
            };
            return _checkin;
        }

        public async Task<Event> GetEventDetails(Guid eventId)
        {
            var _event = await _dbContext.Events.FindAsync(eventId);
            if (_event.IsSubEvents)
            {
                _event.SubEvents = await _dbContext.SubEvents.Where(s => s.EventId == eventId).ToListAsync();
                _event.Guests = await _dbContext.Guests.Where(g => g.EventId == eventId).ToListAsync();
                foreach (var se in _event.SubEvents)
                {
                    se.TotalGuests = _event.Guests
                        .Where(g => !string.IsNullOrWhiteSpace(g.AllowedSubEventsIdsCommaList) && g.AllowedSubEventsIdsCommaList.Contains(se.UniqueId.ToString())).Count();
                }
            }

            return _event;
        }
    }
}
