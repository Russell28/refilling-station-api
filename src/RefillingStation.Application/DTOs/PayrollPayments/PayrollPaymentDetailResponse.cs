namespace RefillingStation.Application.DTOs.PayrollPayments
{
    public sealed record PayrollPaymentDetailResponse(
        int Id,
        int EmployeeId,
        string EmployeeName,
        DateOnly PaidDate,
        decimal AmountPaid,
        string? Notes
    );
}
