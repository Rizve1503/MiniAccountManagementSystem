using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Admin.Configuration.Branches
{
    [Authorize(Roles = "Admin")]
    public class EditBranchModel : PageModel
    {
        private readonly IConfiguration _configuration;
        [BindProperty]
        public BankBranch BranchToEdit { get; set; }
        public SelectList ParentBankOptions { get; set; }
        public SelectList BranchTypeOptions { get; set; }

        public EditBranchModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await LoadParentBankOptions();
            await LoadBranchTypeOptions();
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Action", "GET_BRANCH_BY_ID");
                cmd.Parameters.AddWithValue("@BranchId", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        BranchToEdit = new BankBranch
                        {
                            Id = (int)reader["Id"],
                            BranchName = reader["BranchName"].ToString(),
                            AccountNumber = reader["AccountNumber"]?.ToString(),
                            AccountType = reader["AccountType"]?.ToString(),
                            Address = reader["Address"]?.ToString(),
                            BankId = (int)reader["BankId"]
                        };
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
            if (!ModelState.IsValid)
            {
                await LoadParentBankOptions();
                await LoadBranchTypeOptions();
                return Page();
            }

            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Action", "UPDATE_BRANCH");
                cmd.Parameters.AddWithValue("@BranchId", BranchToEdit.Id);
                cmd.Parameters.AddWithValue("@BranchName", BranchToEdit.BranchName);
                cmd.Parameters.AddWithValue("@AccountNumber", (object)BranchToEdit.AccountNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AccountType", (object)BranchToEdit.AccountType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)BranchToEdit.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BankId", BranchToEdit.BankId);
                await cmd.ExecuteNonQueryAsync();
            }
            return RedirectToPage("./Index");
        }

        private async Task LoadBranchTypeOptions()
        {
            var types = new List<BranchType>();
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_ManageBranchTypes", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Action", "GETALL");

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        types.Add(new BranchType { Id = (int)reader["Id"], TypeName = reader["TypeName"].ToString() });
                    }
                }
            }
            BranchTypeOptions = new SelectList(types, "TypeName", "TypeName");
        }

        // Reusing the helper method from CreateBranchModel
        private async Task LoadParentBankOptions()
        {
            var banks = new List<Bank>();
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
                        banks.Add(new Bank { Id = (int)reader["Id"], BankName = reader["BankName"].ToString() });
                    }
                }
            }
            ParentBankOptions = new SelectList(banks, "Id", "BankName");
        }
    }
}