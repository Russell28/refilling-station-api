using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IExpenseRepository
    {
        Task<Expense?> GetByIdAsync(int id);
        Task<List<Expense>> GetAllAsync();
        Task AddAsync(Expense entity);
        Task DeleteAsync(Expense entity);
        Task SaveChangesAsync();
    }
}
