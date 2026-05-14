namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record PayrollBreakdownItemResponse(
        int EmployeeId,
        string EmployeeName,
        decimal Earned,
        decimal Paid,
        decimal Owed
    );
}
