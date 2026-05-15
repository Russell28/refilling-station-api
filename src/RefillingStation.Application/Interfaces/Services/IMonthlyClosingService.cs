using RefillingStation.Application.DTOs.MonthlyClosing;
using RefillingStation.Application.DTOs.Reports.MonthlySummary;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IMonthlyClosingService
    {
        Task<MonthlySummaryResponse> CreateOrUpdateAsync(MonthlyClosingRequest request);
    }
}
