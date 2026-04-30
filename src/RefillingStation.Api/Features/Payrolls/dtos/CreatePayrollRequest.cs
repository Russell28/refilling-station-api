namespace RefillingStation.Api.Features.Payrolls.dtos
{
    public class CreatePayrollRequest
    {
        public DateTime EarnedDate { get; set; }
        public DateTime? PaidDate { get; set; } 
        public int EmployeeId { get; set; }
        public decimal SalaryAmount { get; set; }
        public decimal CashPaid { get; set; }
        public string? Notes { get; set; }
    }
}
