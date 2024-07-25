namespace EventQR.Models.Reports
{
    public class ReportView
    {
        public string GuestId { get; set; }
        public string EventId { get; set; }
        public string SubEventId { get; set; }
        public string AllowedSubEventsIdsCommaList { get; set; }
        public List<EventQR.Models.SubEvent> SubEvents { get; set; }
        public List<EventQR.Models.EventGuest> Guests { get; set; }
    }
}
