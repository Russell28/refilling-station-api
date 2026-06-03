using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IUserRepository : IReadRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
        void Remove(User user);
        Task<int> SaveChangesAsync();
    }
}
