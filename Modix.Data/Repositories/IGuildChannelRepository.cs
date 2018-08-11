using System;
using System.Threading.Tasks;

using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    public interface IGuildChannelRepository
    {
        /// <summary>
        /// Begins a new transaction to create channels within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Creates a new set of channel data within the repository.
        /// </summary>
        /// <param name="data">The initial set of channel data to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task CreateAsync(GuildChannelCreationData data);

        /// <summary>
        /// Attempts to update information about a channel, based on a pair of its ID value.
        /// </summary>
        /// <param name="channelId">The <see cref="GuildChannelEntity.ChannelId"/> value of the user guild data to be updated.</param>
        /// <param name="updateAction">An action to be invoked to perform the requested update.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether the requested update succeeded (I.E. whether the specified data record exists).
        /// </returns>
        Task<bool> TryUpdateAsync(ulong channelId, Action<GuildChannelMutationData> updateAction);
    }
}
