using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventQR.Models
{
    public class TicketScanner
    {
        [Key]
        public int UniqueId { get; set; }
        public Guid? UserLoginId { get; set; }
        public Guid? EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        public string Name { get; set; } = string.Empty;

        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]

        public string Mobile1 { get; set; } = string.Empty;
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]

        public string Mobile2 { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [RegularExpression(@"^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$", ErrorMessage = "Email is not in proper format")]
      public string EmailId { get; set; } = string.Empty;

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public DateTime CreatedData { get; set; }
        public DateTime LastUpdatedData { get; set; }
    }
}
