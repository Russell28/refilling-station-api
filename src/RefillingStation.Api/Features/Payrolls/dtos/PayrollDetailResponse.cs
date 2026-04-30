namespace RefillingStation.Api.Features.Payrolls.dtos
{
    public record PayrollDetailResponse(
        int Id,
        DateTime EarnedDate,
        DateTime? PaidDate,
        int EmployeeId,
        string EmployeeName,
        decimal SalaryAmount,
        decimal CashPaid,
        string? Notes
    );
}
