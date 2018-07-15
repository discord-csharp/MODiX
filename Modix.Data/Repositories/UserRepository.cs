using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

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
        public async Task CreateAsync(UserCreationData user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var entity = user.ToEntity();
            entity.Created = DateTimeOffset.Now;

            await ModixContext.Users.AddAsync(entity);

            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(ulong userId)
            => ModixContext.Users.AsNoTracking()
                .AnyAsync(x => x.Id == (long)userId);

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(ulong userId, Action<UserMutationData> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var entity = await ModixContext.Users
                .SingleOrDefaultAsync(x => x.Id == (long)userId);

            if (entity == null)
                return false;

            var mutation = UserMutationData.FromEntity(entity);
            updateAction.Invoke(mutation);
            mutation.ApplyTo(entity);

            ModixContext.UpdateProperty(entity, x => x.Nickname);
            ModixContext.UpdateProperty(entity, x => x.LastSeen);

            await ModixContext.SaveChangesAsync();

            return true;
        }
    }
}
