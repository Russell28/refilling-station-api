namespace RefillingStation.Application.DTOs.Reports
{
    public sealed record TripReportItem(
        int Id,
        DateOnly Date,
        int TripNumber,
        decimal CollectedQty,
        decimal LoadedQty,
        decimal DeliveredQty,
        decimal FreeQty,
        decimal ReturnedQty,
        decimal ReplacementQty,
        decimal ActualCashCollected
    );
    
}
