namespace RefillingStation.Application.DTOs.CustomerDebts
{
    public sealed record CustomerDebtResponse
    (
        int Id,
        DateTime Date,
        int CustomerId,
        string CustomerName,
        decimal Amount,
        string? Notes
    );
}
