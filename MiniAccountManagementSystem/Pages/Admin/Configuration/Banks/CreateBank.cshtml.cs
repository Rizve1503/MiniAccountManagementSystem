using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Admin.Configuration.Banks
{
    [Authorize(Roles = "Admin")]
    public class CreateBankModel : PageModel
    {
        private readonly IConfiguration _configuration;
        [BindProperty]
        public Bank NewBank { get; set; }

        public CreateBankModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            try
            {
                string connString = _configuration.GetConnectionString("DefaultConnection");
                using (var conn = new SqlConnection(connString))
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@Action", "CREATE_BANK");
                    cmd.Parameters.AddWithValue("@BankName", NewBank.BankName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("NewBank.BankName", ex.Message);
                return Page();
            }
            return RedirectToPage("./Index");
        }
    }
}