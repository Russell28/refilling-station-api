namespace RefillingStation.Application.DTOs.Reports
{
    public sealed record PayrollReportItem(
        int Id,
        DateOnly EarnedDate,
        int EmployeeId,
        string EmployeeName,
        decimal SalaryAmount,
        decimal CashPaid
    );
}
