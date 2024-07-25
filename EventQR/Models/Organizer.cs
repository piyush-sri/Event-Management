using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventQR.Models
{
    public class Organizer
    {
        [Key]
        public Guid UniqueId { get; set; }
        public Guid OrganizerUserId { get; set; }
        [Required]
        [DisplayName("Organization Name")]
        public string OrganizationName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        [Required]
        public string Phone1 { get; set; } = string.Empty;
        public string Phone2 { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string OfficeAddress { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        [NotMapped]
        public IFormFile? ProfileImage { get; set; }
        public string ProfileImageName { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? LogoImage { get; set; }
        public string LogoImageName { get; set; } = string.Empty;

    }
}
