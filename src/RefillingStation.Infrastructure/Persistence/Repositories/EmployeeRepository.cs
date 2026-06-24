using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context) : base(context) { }

        public override async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .OrderBy(e => e.FirstName)
                .ToListAsync();
        }

        public async Task<List<Employee>> GetActiveAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(e => e.FirstName)
                .ToListAsync();
        }
    }
}
