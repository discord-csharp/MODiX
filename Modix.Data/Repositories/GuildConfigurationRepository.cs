using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="GuildConfigEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IGuildConfigurationRepository
    {
        /// <summary>
        /// Returns a value representing whether or not this guild has a minimum number of days
        /// before a person can be promoted based on the date they joined the guild.
        /// </summary>
        /// <param name="guildId">The id of the guild to retrieve information about.</param>
        /// <returns>True if a minimum is set; otherwise, false.</returns>
        Task<bool> HasMinimumDaysBeforePromotion(ulong guildId);
        /// <summary>
        /// Returns an integer value representing the number of days a user must be
        /// a member of the guild before they can be promoted.
        /// </summary>
        /// <param name="guildId">The id of the guild to retrieve information about.</param>
        Task<int> GetMinimumDaysBeforePromotion(ulong guildId);
        /// <summary>
        /// Sets the minimum number of days a user must be a member of the guild
        /// before they can be promoted and saves the value to the database.
        /// </summary>
        /// <param name="guildId">The ID of the guild to configure.</param>
        /// <param name="days">The number of days. Provide 0 to disable.</param>
        /// <returns>A task which completes when the configuration has been updated.</returns>
        Task SetMinimumDaysBeforePromotion(ulong guildId, int days);
    }

    /// <inheritdoc />
    public class GuildConfigurationRepository : RepositoryBase, IGuildConfigurationRepository
    {
        public GuildConfigurationRepository(ModixContext modixContext)
            : base(modixContext) { }

        /// <inheritdoc />
        public async Task<bool> HasMinimumDaysBeforePromotion(ulong guildId) =>
            (await GetMinimumDaysBeforePromotion(guildId)) > 0;

        /// <inheritdoc />
        public Task<int> GetMinimumDaysBeforePromotion(ulong guildId)
            => ModixContext.GuildConfigurations.AsNoTracking()
                .Where(x => x.GuildId == guildId)
                .Select(x => x.MinimumDaysBeforePromotion)
                .FirstOrDefaultAsync();

        /// <inheritdoc />
        public async Task SetMinimumDaysBeforePromotion(ulong guildId, int days)
        {
            // Enforce no negative values
            days = days >= 0 ? days : 0;
            var guildConfig = await ModixContext.GuildConfigurations.FindAsync(guildId) ?? new GuildConfigEntity();
            guildConfig.MinimumDaysBeforePromotion = days;
            ModixContext.GuildConfigurations.Update(guildConfig);
            await ModixContext.SaveChangesAsync();
        }
    }
}
