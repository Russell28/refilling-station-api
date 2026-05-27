namespace RefillingStation.Domain.Entities
{
    public class PayrollEntry : BaseEntity
    {
        public DateOnly EarnedDate { get; set; }
        public DateOnly? PaidDate { get; set; }
        public int EmployeeId { get; set; }
        public decimal SalaryAmount { get; set; }
        public decimal CashPaid { get; set; }
        public string? Notes { get; set; }

        // Relationships
        public Employee Employee { get; set; } = null!;
    }
}
