using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniAccountManagementSystem.Data
{
    public class VoucherDetail
    {
        public int Id { get; set; }

        // Foreign key linking back to the master voucher
        public int VoucherMasterId { get; set; }

        [Required]
        [Display(Name = "Account")]
        public int ChartOfAccountId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Specifies the exact database column type
        [Display(Name = "Debit")]
        public decimal DebitAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Credit")]
        public decimal CreditAmount { get; set; }
    }
}