namespace RefillingStation.Application.DTOs.Reports
{
    public sealed record PayrollPaymentReportItem(
        int Id,
        int EmployeeId,
        string EmployeeName,
        DateOnly PaidDate,
        decimal AmountPaid
    );
}
