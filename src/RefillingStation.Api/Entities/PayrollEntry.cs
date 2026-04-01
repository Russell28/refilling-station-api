namespace RefillingStation.Api.Entities
{
    public class PayrollEntry : BaseEntity
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
