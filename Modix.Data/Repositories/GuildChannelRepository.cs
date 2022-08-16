using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="GuildChannelEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IGuildChannelRepository
    {
        /// <summary>
        /// Begins a new transaction to create channels within the repository.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which may be used to cancel the returned <see cref="Task"/> before it completes.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new set of channel data within the repository.
        /// </summary>
        /// <param name="data">The initial set of channel data to be created.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which may be used to cancel the returned <see cref="Task"/> before it completes.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task CreateAsync(GuildChannelCreationData data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to update information about a channel, based on its ID value.
        /// </summary>
        /// <param name="channelId">The <see cref="GuildChannelEntity.ChannelId"/> value of the user guild data to be updated.</param>
        /// <param name="updateAction">An action to be invoked to perform the requested update.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which may be used to cancel the returned <see cref="Task"/> before it completes.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether the requested update succeeded (I.E. whether the specified data record exists).
        /// </returns>
        Task<bool> TryUpdateAsync(ulong channelId, Action<GuildChannelMutationData> updateAction, CancellationToken cancellationToken = default);
    }

    /// <inheritdoc />
    public class GuildChannelRepository : RepositoryBase, IGuildChannelRepository
    {
        /// <summary>
        /// Creates a new <see cref="GuildChannelRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public GuildChannelRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync(CancellationToken cancellationToken = default)
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database, cancellationToken);

        /// <inheritdoc />
        public async Task CreateAsync(GuildChannelCreationData data, CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            ModixContext.Set<GuildChannelEntity>().Add(entity);
            await ModixContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> TryUpdateAsync(ulong channelId, Action<GuildChannelMutationData> updateAction, CancellationToken cancellationToken = default)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var entity = await ModixContext.Set<GuildChannelEntity>()
                .Where(x => x.ChannelId == channelId)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
                return false;

            var data = GuildChannelMutationData.FromEntity(entity);
            updateAction.Invoke(data);
            data.ApplyTo(entity);

            ModixContext.UpdateProperty(entity, x => x.Name);

            await ModixContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
