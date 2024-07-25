using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text;

namespace EventQR.Models
{
    public class Event
    {
        [Key]
        public Guid UniqueId { get; set; }

        public Guid EventOrganizerId { get; set; }

        [ForeignKey("EventOrganizerId")]
        [DisplayName("Even Organizer")]
        public Organizer? EvenOrganizer { get; set; }

        [DisplayName("Event Title")]
        public string Title { get; set; } = string.Empty;
        [DisplayName("Event Description")]

        public string Description { get; set; } = string.Empty;

        [DisplayName("Venue (Address)")]
        public string Venue { get; set; } = string.Empty;

        //@ToDo  Rename StartDate to EventStartDate
        [DisplayName("Event Start Date")]
        public DateTime? StartDate { get; set; }

        //@ToDo  Rename EndDate to EventEndDate
        [DisplayName("Event End Date")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// IsSubEvents is a flag include more some events in main events like  Meals , cake cutting, any dance representation or some thing else .  and individual guest can be configured for each sub event.
        /// </summary>
       
        public bool IsSubEvents { get; set; }

        [NotMapped]
        public List<EventGuest>? Guests { get; set; }
        public string Duration
        {
            get
            {
                if (EndDate.HasValue && StartDate.HasValue)
                {
                    var dateDiff = EndDate.Value - StartDate.Value;
                    if (dateDiff.TotalSeconds > 0)
                    {
                        var diff = new EventDuration() { days = dateDiff.Days, hours = dateDiff.Hours, minutes = dateDiff.Minutes };
                        StringBuilder sb = new StringBuilder();
                        if (diff.days > 0) sb.Append(diff.days + " d");
                        if (diff.hours > 0)
                        {
                            if (diff.days > 0)
                                sb.Append(" | ");
                            sb.Append(diff.hours + " h");
                        }
                        if (diff.minutes > 0) sb.Append(" | " + diff.minutes + " m");

                        return sb.ToString();
                    }
                }
                return string.Empty;
            }
        }

        [NotMapped]
        public string Status
        {
            get
            {
                string _status = string.Empty;
                if (StartDate >= DateTime.Now)
                    _status = EventStatus.Scheduled.ToString();
                else if (StartDate <= DateTime.Now && EndDate >= DateTime.Now)
                    _status = EventStatus.InProgress.ToString();
                else _status = EventStatus.Done.ToString();
                return _status;
            }
        }
        public int GuestsCount { get { return Guests != null ? Guests.Count() : 0; } }

        [DisplayName("Days To Go")]
        public int DaysToGo
        {
            get
            {
                if (StartDate != null)
                {
                    var diffOfDates = StartDate.Value - DateTime.Now;
                    return diffOfDates.Days > 0 ? diffOfDates.Days : 0;
                }
                else return 0;
            }
        }

        public List<SubEvent>? SubEvents { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }
        [DisplayName("Last Updated Date")]
        public DateTime LastUpdatedDate { get; set; }
        public string? TicketViewName { get; set; }


        [NotMapped]
        public int TotalGuests { get { return Guests != null ? Guests.Count() : 0; } }
        [NotMapped]
        public int AttendedGuests { get; set; }
    }

    public enum EventStatus { Scheduled, InProgress, Done }

    public class EventDuration
    {
        public int days { get; set; }
        public int hours { get; set; }
        public int minutes { get; set; }
    }
}
