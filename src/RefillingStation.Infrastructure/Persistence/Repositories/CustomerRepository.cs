using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context) : base(context) { }

        public override async Task<List<Customer>> GetAllAsync()
        { 
            Console.WriteLine("DB HIT: Fetching customers from database...");
            return await _context.Customers
                .AsNoTracking()
                .OrderBy(x => x.Name.Contains("Other")) // Push "Other" to the end of the list
                .ThenBy(x => x.Name)
                .Take(100)
                .ToListAsync();
        }
    }
}
