using RefillingStation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefillingStation.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
    }
}
