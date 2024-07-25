namespace EventQR.Models.VM.Reports
{
    public class SubEventGuestCheckInLogs
    {
        public Guid GuestId { get; set; }
        public string GuestName { get; set; }
        public string MobileNumber { get; set; }
        public string SubEventName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime? CheckInTime { get; set; }
    }
}
