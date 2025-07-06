namespace MiniAccountManagementSystem.Data
{
    // This ViewModel is specifically for DISPLAYING voucher details,
    // as it includes properties not in the database table, like AccountName.
    public class VoucherDetailDisplayViewModel
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }
}