using System;
using System.Threading.Tasks;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="ModerationConfigEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IModerationConfigRepository
    {
        /// <summary>
        /// Creates a new configuration within the repository.
        /// </summary>
        /// <param name="data">The data for the configuration to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete</returns>
        Task CreateAsync(ModerationConfigCreationData data);

        /// <summary>
        /// Retrieves information about a moderation configuration, based on its ID.
        /// </summary>
        /// <param name="guildId">The <see cref="ModerationConfigEntity.GuildId"/> value of the configuration to be retried.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested configuration, or null if no such configuration exists.
        /// </returns>
        Task<ModerationConfigSummary> ReadAsync(ulong guildId);

        /// <summary>
        /// Updates data for an existing configuration, based on its ID.
        /// </summary>
        /// <param name="guildId">The <see cref="ModerationConfigEntity.GuildId"/> value of the configuration to be modified.</param>
        /// <param name="updateAction">An action that will perform the desired update.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified configuration could be found).
        /// </returns>
        Task<bool> UpdateAsync(ulong guildId, Action<ModerationConfigMutationData> updateAction);

        /// <summary>
        /// Deletes a moderation configuration, based on its ID.
        /// </summary>
        /// <param name="guildId">The <see cref="ModerationConfigEntity.GuildId"/> value of the configuration to be deleted.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified configuration could be found).
        /// </returns>
        Task<bool> DeleteAsync(ulong guildId);
    }
}
