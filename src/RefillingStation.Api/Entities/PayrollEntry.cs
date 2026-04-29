namespace RefillingStation.Api.Entities
{
    public class PayrollEntry : BaseEntity
    {
        public DateTime EarnedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal SalaryAmount { get; set; }
        public decimal CashPaid { get; set; }
        public string? Notes { get; set; }

        // Relationships
        public Employee Employee { get; set; } = null!;
    }
}
