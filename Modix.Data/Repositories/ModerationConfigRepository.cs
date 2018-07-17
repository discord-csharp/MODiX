using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Moderation;
using Modix.Data.Utilities;

namespace Modix.Data.Repositories
{
    public class ModerationConfigRepository : RepositoryBase, IModerationConfigRepository
    {
        /// <summary>
        /// Creates a new <see cref="ModerationActionRepository"/>.
        /// See <see cref="RepositoryBase(ModixContext)"/> for details.
        /// </summary>
        public ModerationConfigRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public async Task CreateAsync(ModerationConfigCreationData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var entity = data.ToEntity();
            entity.Created = DateTimeOffset.Now;

            await ModixContext.ModerationConfigs.AddAsync(entity);

            await ModixContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public Task<ModerationConfigSummary> ReadAsync(ulong guildId)
        {
            var longId = (long)guildId;
            return ModixContext.ModerationConfigs.AsNoTracking()
                .Where(x => x.GuildId == longId)
                .Select(ModerationConfigSummary.FromEntityProjection)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(ulong guildId, Action<ModerationConfigMutationData> updateAction)
        {
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var entity = await ModixContext.ModerationConfigs
                .Where(x => x.GuildId == (long)guildId)
                .FirstOrDefaultAsync();

            if (entity == null)
                return false;

            var data = ModerationConfigMutationData.FromEntity(entity);
            updateAction.Invoke(data);
            data.ApplyTo(entity);

            ModixContext.UpdateProperty(entity, x => x.MuteRoleId);

            await ModixContext.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(ulong guildId)
        {
            var entity = await ModixContext.ModerationConfigs
                .Where(x => x.GuildId == (long)guildId)
                .FirstOrDefaultAsync();

            if (entity == null)
                return false;

            ModixContext.ModerationConfigs.Remove(entity);

            await ModixContext.SaveChangesAsync();

            return true;
        }
    }
}
