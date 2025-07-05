using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniAccountManagementSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
// This new configuration tells Identity to set up services for BOTH users and roles.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI() // This adds back the default UI pages for login, register, etc.
    .AddDefaultTokenProviders(); // Needed for functions like password reset
builder.Services.AddRazorPages();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during DB seeding");
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
//// Add this entire class definition at the end of Program.cs, after the last '}'
//public static class DbSeeder
//{
//    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
//    {
//        // --- WHAT'S HAPPENING HERE? ---
//        // This is our "seeding" method. It asks the application for the tools it needs
//        // (UserManager and RoleManager) and then performs our setup tasks.

//        // 1. GETTING THE TOOLS WE NEED
//        // We need UserManager to create users and RoleManager to create roles.
//        // These are provided by ASP.NET Identity.
//        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
//        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//        // 2. CREATING THE ROLES
//        // We define the names of the roles we want in our system.
//        string[] roleNames = { "Admin", "Accountant", "Viewer" };

//        // We loop through each role name.
//        foreach (var roleName in roleNames)
//        {
//            // Check if the role already exists in the database.
//            var roleExist = await roleManager.RoleExistsAsync(roleName);
//            if (!roleExist)
//            {
//                // If it doesn't exist, create it.
//                await roleManager.CreateAsync(new IdentityRole(roleName));
//            }
//        }

//        // 3. CREATING THE DEFAULT ADMIN USER
//        // We specify the email for our default admin.
//        var adminEmail = "admin@qtec.com";

//        // Check if a user with this email already exists.
//        var adminUser = await userManager.FindByEmailAsync(adminEmail);

//        // If the admin user doesn't exist...
//        if (adminUser == null)
//        {
//            // ...create a new user object.
//            var newAdminUser = new IdentityUser()
//            {
//                UserName = adminEmail,
//                Email = adminEmail,
//                EmailConfirmed = true // We can pre-confirm the email for the admin.
//            };

//            // ...and create the user in the database with a default password.
//            // IMPORTANT: Change this password in a real application!
//            string password = "Password@123";
//            var result = await userManager.CreateAsync(newAdminUser, password);

//            // If the user was created successfully...
//            if (result.Succeeded)
//            {
//                // ...assign the "Admin" role to them.
//                await userManager.AddToRoleAsync(newAdminUser, "Admin");
//            }
//        }
//    }
//}