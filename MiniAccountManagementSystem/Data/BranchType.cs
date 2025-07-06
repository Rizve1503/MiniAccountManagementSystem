using System.ComponentModel.DataAnnotations;
namespace MiniAccountManagementSystem.Data
{
    public class BranchType
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Branch Type Name")]
        public string TypeName { get; set; }
    }
}