using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id);
        Task<List<Customer>> GetAllAsync();
        Task AddAsync(Customer entity);
        Task DeleteAsync(Customer entity);
        Task SaveChangesAsync();
    }
}
