using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
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
