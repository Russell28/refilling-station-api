namespace RefillingStation.Application.DTOs.Trips
{
    public sealed record TripDetailResponse(
        int Id,
        DateOnly Date,
        int TripNumber,
        int EmployeeId,
        string EmployeeName,
        string? CustomerCategory,
        decimal CollectedQty,
        decimal LoadedQty,
        decimal DeliveredQty,
        decimal FreeQty,
        decimal ReturnedQty,
        decimal ReplacementQty,
        decimal ActualCashCollected,
        bool IsRemitted,
        string? Notes
    );
}
