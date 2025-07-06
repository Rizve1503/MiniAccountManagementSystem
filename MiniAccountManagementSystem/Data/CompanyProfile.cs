using System.ComponentModel.DataAnnotations; // Needed for [Display]

namespace MiniAccountManagementSystem.Data
{
    public class CompanyProfile
    {
        [Required(ErrorMessage = "Company Name is required.")]
        // The [Display] attribute makes our form labels look nice automatically.
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Contact Info (Phone/Email)")]
        public string? ContactInfo { get; set; }

        public string? LogoPath { get; set; }
    }
}