using RefillingStation.Application.DTOs.Payrolls;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface IPayrollService
    {
        Task<List<PayrollDetailResponse>> GetAllAsync();
        Task<PayrollDetailResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(PayrollCreateRequest request);
        Task UpdateAsync(int id, PayrollCreateRequest request);
        Task DeleteAsync(int id);
    }
}
