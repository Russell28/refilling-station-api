using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.PayrollEntries;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IPayrollEntryService
    {
        Task<List<PayrollEntryDetailResponse>> GetAllAsync();
        Task<PayrollEntryDetailResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(PayrollEntryCreateRequest request);
        Task UpdateAsync(int id, PayrollEntryCreateRequest request);
        Task DeleteAsync(int id);
        Task<List<PayrollEntryDetailResponse>> SearchByDateRangeAsync(DateRangeRequest request);

    }
}
