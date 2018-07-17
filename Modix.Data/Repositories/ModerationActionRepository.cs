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
        public async Task<long> CreateAsync(ModerationActionCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();
            entity.Created = DateTimeOffset.Now;

            await ModixContext.ModerationActions.AddAsync(entity);

            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<ModerationActionSummary> ReadAsync(long actionId)
            => ModixContext.ModerationActions.AsNoTracking()
                .Where(x => x.Id == actionId)
                .Select(ModerationActionSummary.FromEntityProjection)
                .FirstOrDefaultAsync();

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(long actionId, Action<ModerationActionMutationData> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var entity = await ModixContext.ModerationActions
                .Where(x => x.Id == actionId)
                .FirstOrDefaultAsync();

            if (entity == null)
                return false;

            var data = ModerationActionMutationData.FromEntity(entity);
            updateAction.Invoke(data);
            data.ApplyTo(entity);

            ModixContext.UpdateProperty(entity, x => x.InfractionId);

            await ModixContext.SaveChangesAsync();

            return true;
        }
    }
}
