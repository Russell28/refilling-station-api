namespace RefillingStation.Application.DTOs.CustomerDebts
{
    public sealed record CustomerDebtDetailResponse
    (
        int Id,
        DateTime Date,
        int CustomerId,
        string CustomerName,
        decimal Amount,
        string? Notes
    );
}
