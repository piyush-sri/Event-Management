using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace EventQR.Models
{
    public class SubEvent
    {
        [Key]
        public Guid UniqueId { get; set; }
        public Guid? EventId { get; set; }
        [ForeignKey("EventId")]
        public Event? Event { get; set; } = null;

        public string SubEventName { get; set; }
        [DisplayName("Start Date Time")]
        public DateTime? StartDateTime { get; set; }
        [DisplayName("End Date Time")]
        public DateTime? EndDateTime { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }


        /// <summary>
        /// This prop is to indicate that an individual guest is allowd for this sub event or not., its not for use for this module directoly.
        /// </summary>
        [NotMapped]
        public bool IsIncludedForThisGuest { get; set; }

        [NotMapped]
        public int TotalGuests { get; set; }
        [NotMapped]
        public int AttendedGuests { get; set; }

    }
}
