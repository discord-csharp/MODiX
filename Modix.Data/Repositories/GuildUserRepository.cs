using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;
using Modix.Data.Utilities;

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

    /// <inheritdoc />
    public class GuildUserRepository : RepositoryBase, IGuildUserRepository
    {
        /// <summary>
        /// Creates a new <see cref="GuildUserRepository"/>.
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
                .AsExpandable()
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
