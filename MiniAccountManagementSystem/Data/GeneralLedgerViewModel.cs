namespace MiniAccountManagementSystem.Data
{
    public class GeneralLedgerViewModel
    {
        public DateTime VoucherDate { get; set; }
        public string VoucherType { get; set; }
        public string Narration { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal RunningBalance { get; set; }
    }
}