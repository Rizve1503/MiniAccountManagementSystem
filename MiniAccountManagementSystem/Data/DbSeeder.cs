using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; // Needed for IServiceProvider
using System;
using System.Threading.Tasks;

namespace MiniAccountManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {

            // 1. GETTING THE TOOLS WE NEED
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 2. CREATING THE ROLES
            string[] roleNames = { "Admin", "Accountant", "Viewer" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 3. CREATING THE DEFAULT ADMIN USER
            var adminEmail = "admin@qtec.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdminUser = new IdentityUser()
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                string password = "Password@123";
                var result = await userManager.CreateAsync(newAdminUser, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
            }
        }
    }
}