using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <inheritdoc />
    public class GuildUserRepository : RepositoryBase, IGuildUserRepository
    {
        /// <summary>
        /// Creates a new <see cref="ModerationActionRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public GuildUserRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public Task<IRepositoryTransaction> BeginCreateTransactionAsync()
            => _createTransactionFactory.BeginTransactionAsync(ModixContext.Database);

        /// <inheritdoc />
        public async Task CreateAsync(GuildUserCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var longUserId = (long)data.UserId;

            if(!(await ModixContext.Users.AsNoTracking()
                .AnyAsync(x => x.Id == longUserId)))
            {
                var userEntity = data.ToUserEntity();

                await ModixContext.Users.AddAsync(userEntity);
                await ModixContext.SaveChangesAsync();
            }

            var guildDataEntity = data.ToGuildDataEntity();

            await ModixContext.GuildUsers.AddAsync(guildDataEntity);
            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public Task<GuildUserSummary> ReadSummaryAsync(ulong userId, ulong guildId)
        {
            var longUserId = (long)userId;
            var longGuildId = (long)guildId;

            return ModixContext.GuildUsers.AsNoTracking()
                .Where(x => x.UserId == longUserId)
                .Where(x => x.GuildId == longGuildId)
                .Select(GuildUserSummary.FromEntityProjection)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<bool> TryUpdateAsync(ulong userId, ulong guildId, Action<GuildUserMutationData> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var longUserId = (long)userId;
            var longGuildId = (long)guildId;

            var entity = await ModixContext.GuildUsers
                .Where(x => x.UserId == longUserId)
                .Where(x => x.GuildId == longGuildId)
                .Include(x => x.User)
                .FirstOrDefaultAsync();

            if(entity == null)
                return false;

            var data = GuildUserMutationData.FromEntity(entity);
            updateAction.Invoke(data);
            data.ApplyTo(entity);

            ModixContext.UpdateProperty(entity.User, x => x.Username);
            ModixContext.UpdateProperty(entity.User, x => x.Discriminator);
            ModixContext.UpdateProperty(entity, x => x.Nickname);
            ModixContext.UpdateProperty(entity, x => x.LastSeen);

            await ModixContext.SaveChangesAsync();

            return true;
        }

        private static readonly RepositoryTransactionFactory _createTransactionFactory
            = new RepositoryTransactionFactory();
    }
}
