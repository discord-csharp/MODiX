using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Nito.AsyncEx;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <inheritdoc />
    public class UserRepository : RepositoryBase, IUserRepository
    {
        /// <summary>
        /// Creates a new <see cref="ModerationActionRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public UserRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public async Task CreateOrUpdateAsync(ulong userId, Action<UserMutationData> updateAction)
        {
            if(updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var longUserId = (long)userId;

            var createLock = await _createLock.LockAsync();

            var entity = await ModixContext.Users
                .SingleOrDefaultAsync(x => x.Id == longUserId);

            if (entity != null)
            {
                createLock.Dispose();
                createLock = null;
            }
            else
            {
                entity = new UserEntity()
                {
                    Id = longUserId,
                    FirstSeen = DateTimeOffset.Now
                };

                await ModixContext.Users.AddAsync(entity);
            }

            var mutation = UserMutationData.FromEntity(entity);
            updateAction.Invoke(mutation);
            mutation.ApplyTo(entity);

            entity.LastSeen = DateTimeOffset.Now;

            if(createLock == null)
            {
                ModixContext.UpdateProperty(entity, x => x.Username);
                ModixContext.UpdateProperty(entity, x => x.Discriminator);
                ModixContext.UpdateProperty(entity, x => x.Nickname);
                ModixContext.UpdateProperty(entity, x => x.LastSeen);
            }

            await ModixContext.SaveChangesAsync();

            if (createLock != null)
                createLock.Dispose();
        }

        public Task<UserSummary> ReadAsync(ulong userId)
        {
            var longUserId = (long)userId;

            return ModixContext.Users.AsNoTracking()
                .Where(x => x.Id == longUserId)
                .Select(UserSummary.FromEntityProjection)
                .FirstOrDefaultAsync();
        }

        private static readonly AsyncLock _createLock
            = new AsyncLock();
    }
}
