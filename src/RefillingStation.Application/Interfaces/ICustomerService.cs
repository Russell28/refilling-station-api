using RefillingStation.Application.DTOs.Customers;

namespace RefillingStation.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerListItemResponse>> GetAllAsync();
        Task<CustomerListItemResponse> GetByIdAsync(int id);
        Task<int> CreateAsync(CustomerCreateRequest request);
        Task UpdateAsync(int id, CustomerCreateRequest request);
        Task DeleteAsync(int id);
    }
}
