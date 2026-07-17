namespace RefillingStation.Application.DTOs.PayrollEntries
{
    public sealed record PayrollEntryDetailResponse(
        int Id,
        DateOnly EarnedDate,
        int EmployeeId,
        string EmployeeName,
        decimal SalaryAmount,
        string? Notes
    );
}
