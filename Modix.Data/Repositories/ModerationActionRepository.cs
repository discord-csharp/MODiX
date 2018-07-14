using System;
using System.Linq;
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
        public async Task<long> InsertAsync(ModerationActionData action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var entity = action.ToEntity();

            entity.Created = DateTimeOffset.Now;

            await ModixContext.ModerationActions.AddAsync(entity);

            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public async Task<ModerationActionSummary> GetAsync(long actionId)
            => await ModixContext.ModerationActions.AsNoTracking()
                .Where(x => x.Id == actionId)
                .Select(ModerationActionSummary.FromEntityProjection)
                .FirstOrDefaultAsync();

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
