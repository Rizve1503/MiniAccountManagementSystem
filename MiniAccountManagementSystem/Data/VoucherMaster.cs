using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Data
{
    public class VoucherMaster
    {
        // This corresponds to the primary key in the database
        public int Id { get; set; }

        [Required]
        [Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; }

        [Required]
        [Display(Name = "Voucher Type")]
        public string VoucherType { get; set; }

        [Display(Name = "Reference No.")]
        public string? ReferenceNo { get; set; }

        public string? Narration { get; set; }

        // These fields are for tracking and are usually not displayed on the form
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}