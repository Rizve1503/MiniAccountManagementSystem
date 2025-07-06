using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Reports
{
    [Authorize(Roles = "Admin, Accountant, Viewer")]
    public class GeneralLedgerModel : PageModel
    {
        private readonly IConfiguration _configuration;

        // --- Properties for Filters ---
        // We use [BindProperty(SupportsGet = true)] so we can read these values from the URL (GET request).
        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);

        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [BindProperty(SupportsGet = true)]
        public int SelectedAccountId { get; set; }

        // --- Properties for Display ---
        public SelectList ChartOfAccountList { get; set; }
        public List<GeneralLedgerViewModel> LedgerEntries { get; set; } = new();
        public decimal OpeningBalance { get; set; }
        public string SelectedAccountName { get; set; }

        public GeneralLedgerModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            await LoadChartOfAccounts();

            // Only run the report if an account has been selected.
            if (SelectedAccountId > 0)
            {
                string connString = _configuration.GetConnectionString("DefaultConnection");
                using (var conn = new SqlConnection(connString))
                {
                    await conn.OpenAsync();
                    var cmd = new SqlCommand("sp_GetGeneralLedger", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@StartDate", StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", EndDate);
                    cmd.Parameters.AddWithValue("@ChartOfAccountId", SelectedAccountId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Read the FIRST result set (Opening Balance)
                        if (await reader.ReadAsync())
                        {
                            OpeningBalance = (decimal)reader["OpeningBalance"];
                        }

                        // Move to the SECOND result set (Ledger Transactions)
                        await reader.NextResultAsync();

                        while (await reader.ReadAsync())
                        {
                            LedgerEntries.Add(new GeneralLedgerViewModel
                            {
                                VoucherDate = (DateTime)reader["VoucherDate"],
                                VoucherType = reader["VoucherType"].ToString(),
                                Narration = reader["Narration"]?.ToString(),
                                DebitAmount = (decimal)reader["DebitAmount"],
                                CreditAmount = (decimal)reader["CreditAmount"],
                                RunningBalance = (decimal)reader["RunningBalance"]
                            });
                        }
                    }
                }
            }
        }

        // Helper method to load accounts for the dropdown
        private async Task LoadChartOfAccounts()
        {
            var accounts = new List<ChartOfAccount>();
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_GetChartOfAccounts", conn) { CommandType = CommandType.StoredProcedure };
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        accounts.Add(new ChartOfAccount { Id = (int)reader["Id"], AccountName = $"{reader["AccountCode"]} - {reader["AccountName"]}" });
                    }
                }
            }
            ChartOfAccountList = new SelectList(accounts, "Id", "AccountName");
            // Find the name of the currently selected account for the report header
            SelectedAccountName = accounts.FirstOrDefault(a => a.Id == SelectedAccountId)?.AccountName ?? "N/A";
        }
    }
}