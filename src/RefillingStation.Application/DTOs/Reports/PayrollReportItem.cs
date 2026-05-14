namespace RefillingStation.Application.DTOs.Reports
{
    public sealed record PayrollReportItem(
        int Id,
        DateTime EarnedDate,
        int EmployeeId,
        string EmployeeName,
        decimal SalaryAmount,
        decimal CashPaid
    );
}
