using Microsoft.EntityFrameworkCore;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Domain.Entities;

namespace RefillingStation.Infrastructure.Persistence.Repositories
{
    public class UserRepository : ReadRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) {}

        public async Task AddAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _context.AddAsync(user);
        }

        public async Task<User?> GetByUsernameAsync(string username)
            => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        public void Remove(User user)
            => _context.Users.Remove(user);

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
