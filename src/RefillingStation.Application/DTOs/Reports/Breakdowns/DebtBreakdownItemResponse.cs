namespace RefillingStation.Application.DTOs.Reports.Breakdowns
{
    public sealed record DebtBreakdownItemResponse(
        int CustomerId,
        string CustomerName,
        decimal DebtCreated,
        decimal DebtPayment,
        decimal Balance,
        DateOnly LatestTransactionDate
    );
}
