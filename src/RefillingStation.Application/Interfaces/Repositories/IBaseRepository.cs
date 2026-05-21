using System.Linq.Expressions;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Remove(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate); // Accepts Expression (x => x.Id == Id)
        Task<int> SaveChangesAsync();
    }
}
