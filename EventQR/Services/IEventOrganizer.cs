using EventQR.Models;

namespace EventQR.Services
{
    public interface IEventOrganizer
    {
        public Organizer GetLoggedInEventOrg();
        public Task<Event> GetEventById(Guid _eventId);
        public void SetCurrentEvent(Event _event);
        public Event GetCurrentEvent();
        public void SetLoggedInEventOrgSession(Organizer _org);
        public Organizer GetLoggedInEventOrgSession();
        public Task<EventGuest> GetAllDetailsForGuest(Guid guestId);
        public Task<GuestCheckIn> GetGuestCheckInDto(Guid guestId);
        public Task<Event> GetEventDetails(Guid eventId);

    }
}
