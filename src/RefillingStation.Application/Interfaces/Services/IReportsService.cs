using RefillingStation.Application.DTOs.Reports.Dashboard;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IReportsService
    {
        // Daily Summary
        Task<object> GetDailySummaryAsync(DateOnly date, bool isAdmin);
        // Dashboard
        Task<DashboardResponse> GetDashboardAsync(DateOnly startDate, DateOnly endDate);
        // Monthly Summary
        Task<MonthlySummaryResponse> GetMonthlySummaryAsync(string monthYear);
    }
}
