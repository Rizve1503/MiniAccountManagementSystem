using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectList
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Admin.Configuration.ChartOfAccounts
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Property to hold the final tree structure for display
        public List<ChartOfAccount> AccountTree { get; set; }

        // Property to hold the flat list of accounts for the dropdown
        public SelectList ParentAccountList { get; set; }

        // Property to bind the data from the 'Create New Account' form
        [BindProperty]
        public ChartOfAccount NewAccount { get; set; }

        public async Task OnGetAsync()
        {
            // First, load all accounts from the database
            var allAccounts = await LoadAllAccountsAsync();

            // Then, build the hierarchical tree from the flat list
            AccountTree = BuildTree(allAccounts);

            // Finally, create a SelectList for the parent account dropdown
            ParentAccountList = new SelectList(allAccounts, "Id", "AccountName");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // If the form is not valid, we need to reload the page data
                await OnGetAsync();
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_ManageChartOfAccounts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Action", "CREATE");
                    command.Parameters.AddWithValue("@AccountCode", NewAccount.AccountCode);
                    command.Parameters.AddWithValue("@AccountName", NewAccount.AccountName);
                    command.Parameters.AddWithValue("@AccountType", NewAccount.AccountType);
                    command.Parameters.AddWithValue("@ParentAccountId", (object)NewAccount.ParentAccountId ?? DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage(); // Redirect to refresh the list
        }

        // --- Helper Methods ---

        private async Task<List<ChartOfAccount>> LoadAllAccountsAsync()
        {
            var accounts = new List<ChartOfAccount>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_GetChartOfAccounts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            accounts.Add(new ChartOfAccount
                            {
                                Id = (int)reader["Id"],
                                AccountCode = reader["AccountCode"].ToString(),
                                AccountName = reader["AccountName"].ToString(),
                                AccountType = reader["AccountType"].ToString(),
                                ParentAccountId = reader["ParentAccountId"] as int?
                            });
                        }
                    }
                }
            }
            return accounts;
        }

        private List<ChartOfAccount> BuildTree(List<ChartOfAccount> flatList)
        {
            var tree = new List<ChartOfAccount>();
            var lookup = flatList.ToDictionary(a => a.Id, a => a);

            foreach (var item in flatList)
            {
                if (item.ParentAccountId.HasValue && lookup.ContainsKey(item.ParentAccountId.Value))
                {
                    // If it's a child, find its parent in the lookup and add this item to the parent's Children list.
                    lookup[item.ParentAccountId.Value].Children.Add(item);
                }
                else
                {
                    // If it has no parent, it's a root node. Add it directly to the tree.
                    tree.Add(item);
                }
            }
            return tree;
        }
    }
}