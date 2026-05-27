namespace RefillingStation.Application.DTOs.Payrolls
{
    public sealed class PayrollCreateRequest
    {
        public DateOnly EarnedDate { get; set; }
        public DateOnly? PaidDate { get; set; } 
        public int EmployeeId { get; set; }
        public decimal SalaryAmount { get; set; }
        public decimal CashPaid { get; set; }
        public string? Notes { get; set; }
    }
}
