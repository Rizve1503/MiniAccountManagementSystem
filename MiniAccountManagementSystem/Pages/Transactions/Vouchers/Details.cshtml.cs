using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Transactions.Vouchers
{
    [Authorize(Roles = "Admin, Accountant, Viewer")]
    public class DetailsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        // Property to hold the Master part of the voucher
        public VoucherMaster VoucherMaster { get; set; }

        // Property to hold the list of Detail lines
        public List<VoucherDetailDisplayViewModel> VoucherDetails { get; set; }

        public DetailsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            VoucherDetails = new List<VoucherDetailDisplayViewModel>();
            string connString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_GetVoucherDetailsById", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@VoucherMasterId", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // --- Read the FIRST result set (The Master Record) ---
                    if (await reader.ReadAsync())
                    {
                        VoucherMaster = new VoucherMaster
                        {
                            Id = (int)reader["Id"],
                            VoucherDate = (DateTime)reader["VoucherDate"],
                            VoucherType = reader["VoucherType"].ToString(),
                            ReferenceNo = reader["ReferenceNo"]?.ToString(),
                            Narration = reader["Narration"]?.ToString(),
                            CreatedBy = reader["CreatedBy"]?.ToString(),
                            CreatedDate = (DateTime)reader["CreatedDate"]
                        };
                    }
                    else
                    {
                        // If the master record isn't found, the voucher doesn't exist.
                        return NotFound();
                    }

                    // --- Move to the SECOND result set (The Detail Records) ---
                    await reader.NextResultAsync();

                    while (await reader.ReadAsync())
                    {
                        VoucherDetails.Add(new VoucherDetailDisplayViewModel
                        {
                            AccountCode = reader["AccountCode"].ToString(),
                            AccountName = reader["AccountName"].ToString(),
                            DebitAmount = (decimal)reader["DebitAmount"],
                            CreditAmount = (decimal)reader["CreditAmount"]
                        });
                    }
                }
            }
            return Page();
        }
    }
}