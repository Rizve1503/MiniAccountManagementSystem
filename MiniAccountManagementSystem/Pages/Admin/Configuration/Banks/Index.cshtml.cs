using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Admin.Configuration.Banks
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<Bank> BankList { get; set; } = new List<Bank>();

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Action", "GET_BANKS");
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        BankList.Add(new Bank { Id = (int)reader["Id"], BankName = reader["BankName"].ToString() });
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                string connString = _configuration.GetConnectionString("DefaultConnection");
                using (var conn = new SqlConnection(connString))
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@Action", "DELETE_BANK");
                    cmd.Parameters.AddWithValue("@BankId", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                // Add a model error to display to the user
                ModelState.AddModelError(string.Empty, ex.Message);
                // Reload the page data before showing the page again
                await OnGetAsync();
                return Page();
            }
            return RedirectToPage();
        }
        
    }
}