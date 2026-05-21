using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
    }
}
