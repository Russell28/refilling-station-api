using RefillingStation.Application.DTOs.Reports.Dashboard;

namespace RefillingStation.Application.Interfaces.Repositories.Reports
{
    public interface IDashboardRepository
    {
        Task<DashboardRawData> GetDashboardAsync(DateOnly startDate, DateOnly endDate);
    }
}
