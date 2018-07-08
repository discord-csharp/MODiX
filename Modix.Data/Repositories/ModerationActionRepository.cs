using System;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Moderation;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <inheritdoc />
    public class ModerationActionRepository : RepositoryBase, IModerationActionRepository
    {
        /// <summary>
        /// Creates a new <see cref="ModerationActionRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public ModerationActionRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public async Task<long> InsertAsync(ModerationActionEntity action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            action.Created = DateTimeOffset.Now;

            await ModixContext.ModerationActions.AddAsync(action);

            await ModixContext.SaveChangesAsync();

            return action.Id;
        }

        /// <inheritdoc />
        public async Task SetInfractionAsync(long actionId, long infractionId)
        {
            var action = await ModixContext.ModerationActions
                .SingleAsync(x => x.Id == actionId);

            action.InfractionId = infractionId;

            ModixContext.UpdateProperty(action, x => x.InfractionId);

            await ModixContext.SaveChangesAsync();
        }
    }
}
