namespace RefillingStation.Application.DTOs.PayrollEntries
{
    public sealed class PayrollEntryCreateRequest
    {
        public DateOnly EarnedDate { get; set; }
        public int EmployeeId { get; set; }
        public decimal SalaryAmount { get; set; }
        public string? Notes { get; set; }
    }
}
