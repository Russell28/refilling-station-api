namespace RefillingStation.Application.DTOs.PayrollEntries
{
    public sealed class PayrollEntryCreateRequest
    {
        public DateOnly EarnedDate { get; set; }
        public DateOnly? PaidDate { get; set; } 
        public int EmployeeId { get; set; }
        public decimal SalaryAmount { get; set; }
        public decimal CashPaid { get; set; }
        public string? Notes { get; set; }
    }
}
