namespace RefillingStation.Application.DTOs.Payrolls
{
    public class PayrollImportRowRequest
    {
        public string EarnedDate { get; set; } = string.Empty;
        public string PaidDate { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string SalaryAmount { get; set; } = string.Empty;
        public string CashPaid { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
