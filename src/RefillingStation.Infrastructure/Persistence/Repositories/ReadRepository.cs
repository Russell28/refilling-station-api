using Microsoft.EntityFrameworkCore;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class ReadRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected ReadRepository(AppDbContext context)
        {
            _context = context;
        }
        public virtual async Task<T?> GetByIdAsync(int id)
            => await _context.Set<T>().FindAsync(id);

        public virtual async Task<List<T>> GetAllAsync()
            => await _context.Set<T>()
                .AsNoTracking()
                .OrderBy(e => EF.Property<int>(e, "Id"))
                .ToListAsync();
    }
}
