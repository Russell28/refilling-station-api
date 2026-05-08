using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IPayrollRepository
    {
        Task<PayrollEntry?> GetByIdAsync(int id);
        Task<List<PayrollEntry>> GetAllAsync();
        Task AddAsync(PayrollEntry entity);
        Task DeleteAsync(PayrollEntry entity);
        Task SaveChangesAsync();
    }
}
