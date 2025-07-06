using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniAccountManagementSystem.Data;
using System.Data;
using Microsoft.Data.SqlClient; 
using System.Threading.Tasks;

namespace MiniAccountManagementSystem.Pages.Admin.Configuration
{
    [Authorize(Roles = "Admin")] // Only Admins can access this page
    public class CompanyProfileModel(IConfiguration configuration) : PageModel
    {
        private readonly IConfiguration _configuration = configuration;

        [BindProperty] // This links the model to the form on the page
        public required CompanyProfile Profile { get; set; }

        // This is used to show a success message to the user after saving
        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Profile = new CompanyProfile();
            string? connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (connectionString == null)
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("sp_ManageCompanyProfile", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Action", "GET");

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Profile.CompanyName = reader["CompanyName"].ToString();
                            Profile.Address = reader["Address"].ToString();
                            Profile.ContactInfo = reader["ContactInfo"].ToString();
                            Profile.LogoPath = reader["LogoPath"].ToString();
                        }
                    }
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // If form is invalid, stay on the page
            }

            string? connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (connectionString == null)
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("sp_ManageCompanyProfile", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Action", "SAVE");
                    command.Parameters.AddWithValue("@CompanyName", Profile.CompanyName);
                    command.Parameters.AddWithValue("@Address", Profile.Address ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ContactInfo", Profile.ContactInfo ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }

            StatusMessage = "Company profile has been updated successfully.";
            return RedirectToPage(); // Redirect to the same page to show the message
        }
    }
}