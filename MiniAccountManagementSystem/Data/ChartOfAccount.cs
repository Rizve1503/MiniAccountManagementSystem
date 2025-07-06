using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Data
{
    public class ChartOfAccount
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }

        [Required]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

        [Required]
        [Display(Name = "Account Type")]
        public string AccountType { get; set; }

        [Display(Name = "Parent Account")]
        public int? ParentAccountId { get; set; } // Nullable int for top-level accounts

        // This property is not in the database.
        // We will use it to build our tree structure in memory.
        public List<ChartOfAccount> Children { get; set; } = new List<ChartOfAccount>();
    }
}