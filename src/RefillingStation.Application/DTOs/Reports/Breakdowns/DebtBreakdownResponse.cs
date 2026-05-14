namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record DebtBreakdownResponse(
        decimal TotalDebt,
        IReadOnlyList<DebtBreakdownItemResponse> Items
    );
}
