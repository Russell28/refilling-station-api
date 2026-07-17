using RefillingStation.Domain.Entities;

namespace RefillingStation.Domain.Enitities
{
    public class PayrollPayment : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DateOnly PaidDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string? Notes { get; set; }

        // Relationship
        public Employee Employee { get; set; } = null!;
    }
}
