namespace EventQR.ViewModels.Reports
{
    public class EventReportVM
    {
        public Guid EventId { get; set; }
        public string EventDetails { get; set; }
        public List<GuestVM> Guests { get; set; }
        public List<SubEventVM> SubEvents { get; set; }
    }
}
