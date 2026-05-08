using RefillingStation.Application.DTOs.CustomerDebts;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface ICustomerDebtService
    {
        Task<List<CustomerDebtResponse>> GetAllAsync();
        Task<CustomerDebtResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(CustomerDebtCreateRequest request);
        Task UpdateAsync(int id, CustomerDebtCreateRequest request);
        Task DeleteAsync(int id);
    }
}
