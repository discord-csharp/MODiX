using System;
using System.Threading.Tasks;

using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="UserEntity"/> and <see cref="GuildUserEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IGuildUserRepository
    {
        /// <summary>
        /// Begins a new transaction to create users within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Creates a new set of guild data for a user within the repository.
        /// </summary>
        /// <param name="data">The initial set of guild data to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>A <see cref="Task"/> which will complete when th+e operation is complete.</returns>
        Task CreateAsync(GuildUserCreationData data);

        /// <summary>
        /// Retrieves summary information about a user.
        /// </summary>
        /// <param name="userId">The <see cref="GuildUserEntity.UserId"/> value of the user guild data to be retrieved.</param>
        /// <param name="guildId">The <see cref="GuildUserEntity.GuildId"/> value of the user guild data to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested user guild data, or null if no such user exists.
        /// </returns>
        Task<GuildUserSummary> ReadSummaryAsync(ulong userId, ulong guildId);

        /// <summary>
        /// Attempts to update guild information about a user, based on a pair of user and guild ID values.
        /// </summary>
        /// <param name="userId">The <see cref="GuildUserEntity.UserId"/> value of the user guild data to be updated.</param>
        /// <param name="guildId">The <see cref="GuildUserEntity.GuildId"/> value of the user guild data to be updated.</param>
        /// <param name="updateAction">An action to be invoked to perform the requested update.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether the requested update succeeded (I.E. whether the specified data record exists).
        /// </returns>
        Task<bool> TryUpdateAsync(ulong userId, ulong guildId, Action<GuildUserMutationData> updateAction);
    }
}
