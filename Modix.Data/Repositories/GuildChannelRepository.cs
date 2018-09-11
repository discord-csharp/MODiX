using System;
using System.Linq;
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
        /// Attempts to update information about a channel, based on its ID value.
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
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task CreateAsync(GuildChannelCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();

            await ModixContext.GuildChannels.AddAsync(entity);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> TryUpdateAsync(ulong channelId, Action<GuildChannelMutationData> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var longChannelId = (long)channelId;

            var entity = await ModixContext.GuildChannels
                .Where(x => x.ChannelId == longChannelId)
                .FirstOrDefaultAsync();

            if (entity == null)
                return false;

            var data = GuildChannelMutationData.FromEntity(entity);
            updateAction.Invoke(data);
            data.ApplyTo(entity);

            ModixContext.UpdateProperty(entity, x => x.Name);

            await ModixContext.SaveChangesAsync();

            return true;
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
