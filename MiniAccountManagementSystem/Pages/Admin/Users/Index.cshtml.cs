using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // Needed for .ToListAsync()

namespace MiniAccountManagementSystem.Pages.Admin.Users
{
    // We will add the security attribute here later
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        // This is a "constructor". It runs when the page is created.
        // It asks the application for the UserManager tool and saves it for later use.
        public IndexModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // A property to hold our list of users for the page to display.
        public List<IdentityUser> Users { get; set; }

        // This method runs when the page is first requested (a "GET" request).
        public async Task OnGetAsync()
        {
            // We use the UserManager to get all users from the database and put them in a list.
            Users = await _userManager.Users.ToListAsync();
        }
    }
}