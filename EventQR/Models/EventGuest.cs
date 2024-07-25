using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace EventQR.Models
{
    public class EventGuest
    {
        [Key]
        public Guid UniqueId { get; set; }
        public Guid EventId { get; set; }
        [ForeignKey("EventId")]
        public Event? MyEvent { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        // [Required(ErrorMessage = "Please Input a valid Mobile No.")]
        public string? MobileNo1 { get; set; } = string.Empty;

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]

        public string? MobileNo2 { get; set; } = string.Empty;

        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$", ErrorMessage = "Email is not in proper format")]
        [DisplayName("EmailId")]

        public string? Email { get; set; } = string.Empty;
        public int GuestCount { get; set; } = 1;

        public bool IsInviteAccepted { get; set; }
        public DateTime InviteAcceptedOn { get; set; }
        public bool IsInviteSent { get; set; }
        public DateTime InviteSentOn { get; set; }

        public string? QrCodeImageUri { get; set; } = string.Empty;
        [NotMapped]
        public string BarCodeUrl { get { return "/EventQrImages/" + EventId + "/" + UniqueId.ToString() + ".png"; } }

        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string? AllowedSubEventsIdsCommaList { get; set; }

        [NotMapped]
        public List<SubEvent>? SubEvents { get; set; }

        [NotMapped]
        public List<GuestCheckIn>? CheckInDetails { get; set; }
    }

}
