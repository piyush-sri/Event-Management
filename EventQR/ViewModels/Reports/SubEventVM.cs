namespace EventQR.ViewModels.Reports
{
    public class SubEventVM
    {
        public Guid SubEventId { get; set; } 
        public string SubEventName {  get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime CheckInTime { get; set; }

    }
}
