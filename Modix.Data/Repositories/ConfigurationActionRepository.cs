using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    /// <inheritdoc />
    public class ConfigurationActionRepository : RepositoryBase, IConfigurationActionRepository
    {
        /// <summary>
        /// Constructs a new <see cref="ConfigurationActionRepository"/> from the given dependencies.
        /// See <see cref="RepositoryBase"/> for details.
        /// </summary>
        public ConfigurationActionRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public async Task<long> CreateAsync(ConfigurationActionCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();
            entity.Created = DateTimeOffset.Now;

            await ModixContext.ConfigurationActions.AddAsync(entity);

            await ModixContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(long actionId)
            => ModixContext.ConfigurationActions.AsNoTracking()
                .AnyAsync(x => x.Id == actionId);

        /// <inheritdoc />
        public Task<ConfigurationActionSummary> ReadAsync(long actionId)
            => ModixContext.ConfigurationActions.AsNoTracking()
                .Select(ConfigurationActionSummary.FromEntityProjection)
                .FirstOrDefaultAsync(x => x.Id == actionId);

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(long actionId, Action<ConfigurationActionMutationData> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var entity = await ModixContext.ConfigurationActions
                .Where(x => x.Id == actionId)
                .FirstOrDefaultAsync();

            if (entity == null)
                return false;

            var data = ConfigurationActionMutationData.FromEntity(entity);
            updateAction.Invoke(data);
            data.ApplyTo(entity);

            ModixContext.UpdateProperty(entity, x => x.RoleClaimId);

            await ModixContext.SaveChangesAsync();

            return true;
        }
    }
}
