using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectList
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Admin.Configuration.Branches
{
    [Authorize(Roles = "Admin")]
    public class CreateBranchModel : PageModel
    {
        private readonly IConfiguration _configuration;

        // This will hold the data from the form
        [BindProperty]
        public BankBranch NewBranch { get; set; }
        public SelectList BranchTypeOptions { get; set; }

        // This will hold the options for the <select> dropdown
        public SelectList ParentBankOptions { get; set; }

        public CreateBranchModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method runs when the page is first loaded.
        public async Task<IActionResult> OnGetAsync()
        {
            // We need to load the list of banks to populate the dropdown
            await LoadParentBankOptions();
            await LoadBranchTypeOptions();
            return Page();
        }

        // This method runs when the form is submitted.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // If the form is invalid, we must reload the dropdown options
                // before showing the page again.
                await LoadParentBankOptions();
                await LoadBranchTypeOptions();
                return Page();
            }

            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.AddWithValue("@Action", "CREATE_BRANCH");
                cmd.Parameters.AddWithValue("@BankId", NewBranch.BankId);
                cmd.Parameters.AddWithValue("@BranchName", NewBranch.BranchName);
                cmd.Parameters.AddWithValue("@AccountNumber", (object)NewBranch.AccountNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AccountType", (object)NewBranch.AccountType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)NewBranch.Address ?? DBNull.Value);

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
            // NOTE: We are storing the TypeName (string), not the Id, in the BankBranches table.
            BranchTypeOptions = new SelectList(types, "TypeName", "TypeName");
        }
        // A helper method to keep our code clean (Don't Repeat Yourself principle)
        private async Task LoadParentBankOptions()
        {
            var banks = new List<Bank>();
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Action", "GET_BANKS"); // We use the specific action from our SP

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        banks.Add(new Bank { Id = (int)reader["Id"], BankName = reader["BankName"].ToString() });
                    }
                }
            }
            // Create the SelectList, telling it what property to use for the value (Id) and the text (BankName)
            ParentBankOptions = new SelectList(banks, "Id", "BankName");
        }
    }
}