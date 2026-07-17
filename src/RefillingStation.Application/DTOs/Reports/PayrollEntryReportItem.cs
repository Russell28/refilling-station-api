namespace RefillingStation.Application.DTOs.Reports
{
    public sealed record PayrollEntryReportItem(
        int Id,
        DateOnly EarnedDate,
        int EmployeeId,
        string EmployeeName,
        decimal SalaryAmount
    );
}
