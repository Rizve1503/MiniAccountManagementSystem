using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Admin.Configuration.Branches
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<Bank> BankTree { get; set; } = new List<Bank>();

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
                cmd.Parameters.AddWithValue("@Action", "GET_HIERARCHY");

                var banksDictionary = new Dictionary<int, Bank>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int bankId = (int)reader["BankId"];
                        if (!banksDictionary.ContainsKey(bankId))
                        {
                            banksDictionary[bankId] = new Bank { Id = bankId, BankName = reader["BankName"].ToString() };
                        }
                        if (reader["BranchId"] != DBNull.Value)
                        {
                            banksDictionary[bankId].Branches.Add(new BankBranch
                            {
                                Id = (int)reader["BranchId"],
                                BranchName = reader["BranchName"].ToString(),
                                AccountNumber = reader["AccountNumber"]?.ToString(),
                                AccountType = reader["AccountType"]?.ToString(),
                                Address = reader["Address"]?.ToString(),
                                BankId = bankId
                            });
                        }
                    }
                }
                BankTree = banksDictionary.Values.OrderBy(b => b.BankName).ToList();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_ManageBankHierarchy", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Action", "DELETE_BRANCH");
                cmd.Parameters.AddWithValue("@BranchId", id);
                await cmd.ExecuteNonQueryAsync();
            }
            return RedirectToPage();
        }
    }
}