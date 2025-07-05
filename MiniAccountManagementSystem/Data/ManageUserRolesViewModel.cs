namespace MiniAccountManagementSystem.Data
{
    // This is just a simple container class.
    public class ManageUserRolesViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; } // This will determine if the checkbox is checked
    }
}