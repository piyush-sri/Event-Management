using EventQR.Models.Acc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventQR.Models
{
    public class GuestCheckIn
    {
        [Key]
        public int UniqueId { get; set; }

        public Guid? EventId { get; set; }
        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        public Guid? SubEventId { get; set; }
        [ForeignKey("SubEventId")]
        public SubEvent? SubEvent { get; set; }


        public Guid? GuestId { get; set; }
        [ForeignKey("GuestId")]
        public EventGuest? Guest { get; set; }


        public string? UserLoginId { get; set; }
        [ForeignKey("UserLoginId")]
        public AppUser? ScannerUser { get; set; }

        public DateTime CheckIn { get; set; }
    }
}
