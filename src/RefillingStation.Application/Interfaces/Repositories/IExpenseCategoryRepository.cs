using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IExpenseCategoryRepository
    {
        Task<ExpenseCategory?> GetByIdAsync(int id);
        Task<List<ExpenseCategory>> GetAllAsync();
        Task AddAsync(ExpenseCategory entity);
        Task DeleteAsync(ExpenseCategory entity);
        Task SaveChangesAsync();
    }
}
