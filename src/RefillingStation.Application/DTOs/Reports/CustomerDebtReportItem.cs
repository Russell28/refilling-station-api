namespace RefillingStation.Application.DTOs.Reports
{
    public sealed record CustomerDebtReportItem(
        int Id,
        DateTime Date,
        int CustomerId,
        string CustomerName,
        decimal Amount
    );
}
