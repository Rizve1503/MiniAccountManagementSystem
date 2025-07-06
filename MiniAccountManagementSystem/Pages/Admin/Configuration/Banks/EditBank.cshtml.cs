using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Admin.Configuration.Banks
{
    [Authorize(Roles = "Admin")]
    public class EditBankModel : PageModel
    {
        private readonly IConfiguration _configuration;
        [BindProperty]
        public Bank BankToEdit { get; set; }

        public EditBankModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Action", "GET_BANK_BY_ID");
                cmd.Parameters.AddWithValue("@BankId", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        BankToEdit = new Bank { Id = (int)reader["Id"], BankName = reader["BankName"].ToString() };
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Action", "UPDATE_BANK");
                cmd.Parameters.AddWithValue("@BankId", BankToEdit.Id);
                cmd.Parameters.AddWithValue("@BankName", BankToEdit.BankName);
                await cmd.ExecuteNonQueryAsync();
            }
            return RedirectToPage("./Index");
        }
    }
}