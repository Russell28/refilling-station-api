using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        Task<User?> GetByUsernameAsync(string username);
    }
}
