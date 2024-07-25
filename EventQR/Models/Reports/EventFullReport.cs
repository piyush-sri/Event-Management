namespace EventQR.Models.Reports
{
    public class EventFullReport
    {
        public Event Event { get; set; }
        public List<SubEvent> SubEvents { get; set; }
        public List<Guest> Guests { get; set; }
    }
    public class Event                  
    {
        public string Name { get; set; }

    }

    public class Guest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }                  
        
    }
    public class SubEvent
    { 
        public string Name { get; set; }
        public List<Guest> Guests { get; set; }
    }
    public class GuestReprot
    {
        public List<SubEvent> GetSubEvents { get; set; }
        public List<Guest> Guests { get; set; }
    }
}
