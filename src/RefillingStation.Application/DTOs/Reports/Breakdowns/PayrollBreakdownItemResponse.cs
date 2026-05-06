namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record PayrollBreakdownItemResponse(
        string EmployeeName,
        decimal Earned,
        decimal Paid,
        decimal Owed
    );
}
