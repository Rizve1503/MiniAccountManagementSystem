using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Data;
using System.Data;

namespace MiniAccountManagementSystem.Pages.Transactions.Vouchers
{
    [Authorize(Roles = "Admin, Accountant, Viewer")] // All roles can view the list
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        // This property will hold the list of vouchers to display on the page
        public List<VoucherMaster> VoucherList { get; set; }

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            VoucherList = new List<VoucherMaster>();
            string connString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new SqlConnection(connString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("sp_GetVoucherList", conn) { CommandType = CommandType.StoredProcedure };

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        VoucherList.Add(new VoucherMaster
                        {
                            Id = (int)reader["Id"],
                            VoucherDate = (DateTime)reader["VoucherDate"],
                            VoucherType = reader["VoucherType"].ToString(),
                            ReferenceNo = reader["ReferenceNo"]?.ToString(),
                            Narration = reader["Narration"]?.ToString(),
                            CreatedBy = reader["CreatedBy"]?.ToString()
                        });
                    }
                }
            }
        }
    }
}