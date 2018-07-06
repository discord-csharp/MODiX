using System;
using System.Threading.Tasks;

using Modix.Data.Models;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="User"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Inserts a new <see cref="User"/> into the repository.
        /// </summary>
        /// <param name="user">
        /// The <see cref="User"/> to be inserted.
        /// The <see cref="User.FirstSeen"/> and <see cref="User.LastSeen"/> values are generated automatically.
        /// </param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task InsertAsync(DiscordUser user);

        /// <summary>
        /// Retrieves a <see cref="User"/> from the repositroy, by its <see cref="User.Id"/> value.
        /// </summary>
        /// <param name="id">The <see cref="User.Id"/> value of the <see cref="User"/> to be retrieved.</param>
        /// <returns>A <see cref="Task"/> which will complete when the requested data is available.</returns>
        Task<DiscordUser> GetAsync(ulong id);

        /// <summary>
        /// Updates the <see cref="User.LastSeen"/> value of an existing <see cref="User"/> within the repository,
        /// to <see cref="DateTimeOffset.Now"/>.
        /// </summary>
        /// <param name="id">The <see cref="User.Id"/> value of the <see cref="User"/> to be updated.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task UpdateLastSeenAsync(ulong id);
    }
}
