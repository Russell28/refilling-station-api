namespace RefillingStation.Application.DTOs.Payrolls
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
