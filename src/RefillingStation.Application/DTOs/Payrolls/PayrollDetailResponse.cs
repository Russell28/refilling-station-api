namespace RefillingStation.Application.DTOs.Payrolls
{
    public sealed record PayrollDetailResponse(
        int Id,
        DateOnly EarnedDate,
        DateOnly? PaidDate,
        int EmployeeId,
        string EmployeeName,
        decimal SalaryAmount,
        decimal CashPaid,
        string? Notes
    );
}
