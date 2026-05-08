using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface ICustomerDebtRepository
    {
        Task<CustomerDebtEntry?> GetByIdAsync(int id);
        Task<List<CustomerDebtEntry>> GetAllAsync();
        Task AddAsync(CustomerDebtEntry entity);
        Task DeleteAsync(CustomerDebtEntry entity);
        Task SaveChangesAsync();
    }
}
