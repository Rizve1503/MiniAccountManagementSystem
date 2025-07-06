using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Data
{
    public class Bank
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        // This is not in the DB, it's for building the tree in our code
        public List<BankBranch> Branches { get; set; } = new List<BankBranch>();
    }
}