namespace RefillingStation.Api.Features.Payrolls.dtos
{
    public class CreatePayrollRequest
    {
        public DateTime Date { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal SalaryAmount { get; set; }
        public decimal AdvanceGiven { get; set; }
        public decimal AdvanceDeduction { get; set; }
        public decimal CashPaid { get; set; }
        public string? Notes { get; set; }
    }
}
