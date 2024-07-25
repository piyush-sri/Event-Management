using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventQR.Models.Acc
{
    public class AppUser : IdentityUser

    {
        [NotMapped]
        public string OrganizationName { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^(\d{10})$", ErrorMessage = "The phone number must be exactly 10 digits and contain only numbers.")]
        public override string PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

        public Guid? OrganizerUniqueIdFk { get; set; }
        [ForeignKey(nameof(OrganizerUniqueIdFk))]
        public Guid? EventOrganizer { get; set; }
    }
}
