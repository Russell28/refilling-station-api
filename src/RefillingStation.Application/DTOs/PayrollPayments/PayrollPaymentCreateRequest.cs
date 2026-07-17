namespace RefillingStation.Application.DTOs.PayrollPayments
{
    public sealed class PayrollPaymentCreateRequest
    {
        public int EmployeeId { get; set; }
        public DateOnly PaidDate { get; set; } 
        public decimal AmountPaid { get; set; }
        public string? Notes { get; set; }
    }
}
