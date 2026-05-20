using RefillingStation.Application.DTOs.Reports.Dashboard;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IReportsService
    {
        // Daily Summary
        Task<object> GetDailySummaryAsync(DateTime date, bool isAdmin);
        // Dashboard
        Task<DashboardResponse> GetDashboardAsync(DateTime startDate, DateTime endDate);
        // Monthly Summary
        Task<MonthlySummaryResponse> GetMonthlySummaryAsync(string monthYear);
    }
}
