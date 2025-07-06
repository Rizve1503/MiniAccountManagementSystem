using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Transactions.Vouchers
{
    [Authorize(Roles = "Admin, Accountant")] // Only Admin and Accountant can create vouchers
    public class CreateModel : PageModel
    {
        private readonly IConfiguration _configuration;

        // --- Properties to bind data from the form ---
        [BindProperty]
        public VoucherMaster VoucherMaster { get; set; }

        [BindProperty]
        public List<VoucherDetailViewModel> Details { get; set; }

        // --- Properties for UI elements ---
        public SelectList ChartOfAccountList { get; set; }
        [TempData]
        public string SuccessMessage { get; set; }

        public CreateModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            await LoadChartOfAccounts();
            // Set default values
            VoucherMaster = new VoucherMaster { VoucherDate = DateTime.Today };
            Details = new List<VoucherDetailViewModel> { new(), new() }; // Start with two empty rows
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // --- Server-side Validation ---
            // 1. Remove any empty rows that might have been submitted
            Details.RemoveAll(d => d.ChartOfAccountId == 0 && d.DebitAmount == 0 && d.CreditAmount == 0);

            if (Details.Count == 0)
            {
                ModelState.AddModelError("", "At least one voucher detail line is required.");
            }

            // 2. Business Rule: Total Debits must equal Total Credits
            decimal totalDebit = Details.Sum(d => d.DebitAmount);
            decimal totalCredit = Details.Sum(d => d.CreditAmount);

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError("", $"Total Debit ({totalDebit:N2}) must equal Total Credit ({totalCredit:N2}).");
            }

            if (!ModelState.IsValid)
            {
                await LoadChartOfAccounts();
                return Page(); // Show the page again with error messages
            }

            // --- Save to Database ---
            // 1. Create a DataTable in C# that matches our TVP structure
            var detailsTable = new DataTable();
            detailsTable.Columns.Add("ChartOfAccountId", typeof(int));
            detailsTable.Columns.Add("DebitAmount", typeof(decimal));
            detailsTable.Columns.Add("CreditAmount", typeof(decimal));

            foreach (var item in Details)
            {
                detailsTable.Rows.Add(item.ChartOfAccountId, item.DebitAmount, item.CreditAmount);
            }

            // 2. Execute the Stored Procedure
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_SaveVoucher", conn) { CommandType = CommandType.StoredProcedure };

                cmd.Parameters.AddWithValue("@VoucherDate", VoucherMaster.VoucherDate);
                cmd.Parameters.AddWithValue("@VoucherType", VoucherMaster.VoucherType);
                cmd.Parameters.AddWithValue("@ReferenceNo", (object)VoucherMaster.ReferenceNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Narration", (object)VoucherMaster.Narration ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", User.Identity.Name); // Track who created it

                // Add our special Table-Valued Parameter
                var tvp = cmd.Parameters.AddWithValue("@VoucherDetails", detailsTable);
                tvp.SqlDbType = SqlDbType.Structured; // Specify that it's a structured type
                tvp.TypeName = "dbo.VoucherDetailType"; // The name of the TYPE we created in SQL

                await cmd.ExecuteNonQueryAsync();
            }

            SuccessMessage = "Voucher saved successfully!";
            return RedirectToPage();
        }

        private async Task LoadChartOfAccounts()
        {
            // This is the same logic from our Chart of Accounts page to get the list
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
        }
    }
}