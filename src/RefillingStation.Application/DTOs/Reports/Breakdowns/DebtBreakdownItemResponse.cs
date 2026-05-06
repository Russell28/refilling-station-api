namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record DebtBreakdownItemResponse(
        string CustomerName,
        decimal Amount
    );
}
