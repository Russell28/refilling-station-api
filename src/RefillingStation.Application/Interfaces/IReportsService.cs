using RefillingStation.Application.DTOs.Reports.DailySummary;
using RefillingStation.Application.DTOs.Reports.Dashboard;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;

namespace RefillingStation.Application.Interfaces
{
    public interface IReportsService
    {
        // Daily Summary
        Task<DailySummaryResponse> GetDailySummaryAsync(DateOnly date);
        // Dashboard
        Task<DashboardResponse> GetDashboardAsync(DateTime startDate, DateTime endDate);
        // Monthly Summary
        Task<MonthlySummaryResponse> GetMonthlySummaryAsync(string monthYear);
        Task<MonthlySavedClosingResponse> SaveMonthlySummaryAsync(MonthlyClosingRequest request);
    }
}
