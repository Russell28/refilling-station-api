namespace RefillingStation.Application.DTOs.Reports.MonthlySummary
{
    public sealed record MonthlyClosingReportItem(
        string MonthYear,
        decimal ManagerShare,
        decimal OwnerShare,
        string? Notes,
        DateTime CreatedAt
    );
    
}
