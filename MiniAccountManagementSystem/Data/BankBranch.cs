using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Data
{
    public class BankBranch
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }

        [Display(Name = "Account Number")]
        public string? AccountNumber { get; set; }

        [Display(Name = "Account Type")]
        public string? AccountType { get; set; }

        [Display(Name = "Branch Address")]
        public string? Address { get; set; }

        [Required]
        [Display(Name = "Parent Bank")]
        public int BankId { get; set; }
    }
}