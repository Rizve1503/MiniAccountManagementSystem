using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MiniAccountManagementSystem.Data; // Needed for our ViewModel
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniAccountManagementSystem.Pages.Admin.Users
{
    // We will add security here later
    [Authorize(Roles = "Admin")]
    public class ManageModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public string UserEmail { get; set; }

        // This special property tells the page to bind the form data from the POST request
        // to this list when the form is submitted.
        [BindProperty]
        public List<ManageUserRolesViewModel> ManageUserRoles { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"Unable to find user with ID '{userId}'.");
            }

            UserEmail = user.Email;
            ManageUserRoles = new List<ManageUserRolesViewModel>();

            // Get all possible roles
            foreach (var role in await _roleManager.Roles.ToListAsync())
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                // Check if the user has this role
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                ManageUserRoles.Add(userRolesViewModel);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"Unable to find user with ID '{userId}'.");
            }

            // Get the user's current roles
            var roles = await _userManager.GetRolesAsync(user);
            // Remove all current roles
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user's existing roles.");
                return Page(); // Stay on the page and show error
            }

            // Add the newly selected roles from the form
            result = await _userManager.AddToRolesAsync(user,
                ManageUserRoles.Where(x => x.Selected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user.");
                return Page();
            }

            // Redirect back to the user list after successful update
            return RedirectToPage("./Index");
        }
    }
}