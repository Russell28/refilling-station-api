using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context; // Dependency injection of the database context

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByUsernameAsync(string username)
            => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
}
