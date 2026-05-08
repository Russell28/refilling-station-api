using RefillingStation.Application.DTOs.Customers;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerListItemResponse>> GetAllAsync();
        Task<CustomerListItemResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(CustomerCreateRequest request);
        Task UpdateAsync(int id, CustomerCreateRequest request);
        Task DeleteAsync(int id);
    }
}
