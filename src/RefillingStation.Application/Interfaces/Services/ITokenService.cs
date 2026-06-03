
using RefillingStation.Domain.Entities;

namespace RefillingStation.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
