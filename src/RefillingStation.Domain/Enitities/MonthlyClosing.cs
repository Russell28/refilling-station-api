namespace RefillingStation.Domain.Entities
{
    public class MonthlyClosing : BaseEntity
    {
        public string Month { get; set; } = string.Empty; // yyyy-MM

        public decimal TotalCashCollected { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalPayrollEarned { get; set; }
        public decimal NetProfit { get; set; }

        public decimal ManagerShare { get; set; }
        public decimal OwnerShare { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
