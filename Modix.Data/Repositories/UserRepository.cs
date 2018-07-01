using System.Threading.Tasks;

using Modix.Data.Models;

namespace Modix.Data.Repositories
{
    public interface IUserRepository
    {
        Task InsertAsync(User user);

        Task<User> GetAsync(ulong id);

        Task UpdateLastSeenAsync(ulong id);
    }
}
