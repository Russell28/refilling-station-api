using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        protected BaseRepository(AppDbContext context)
        {
            _context = context;
        }
        public virtual async Task<T?> GetByIdAsync(int id)
            => await _context.Set<T>().FindAsync(id);

        public virtual async Task<List<T>> GetAllAsync()
            => await _context.Set<T>()
                .AsNoTracking()
                .Take(100)
                .OrderByDescending(e => EF.Property<int>(e, "Id"))
                .ToListAsync();

        public virtual async Task AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            await _context.Set<T>().AddAsync(entity);
        }

        public virtual void Remove(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _context.Set<T>().Remove(entity);
        }

        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
