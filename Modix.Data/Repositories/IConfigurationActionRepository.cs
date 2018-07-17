using System;
using System.Threading.Tasks;

using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="ConfigurationActionEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IConfigurationActionRepository
    {
        /// <summary>
        /// Creates a new configuration action within the repository.
        /// </summary>
        /// <param name="data">The data for the configuration action to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="ConfigurationActionEntity.Id"/> value assigned to the new configuration action.
        /// </returns>
        Task<long> CreateAsync(ConfigurationActionCreationData data);

        /// <summary>
        /// Checks whether a configuration action exists, based on its ID.
        /// </summary>
        /// <param name="actionId">The <see cref="ModerationActionEntity.Id"/> value of the configuration action to check for.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether or not the configuration action exists.
        /// </returns>
        Task<bool> ExistsAsync(long actionId);

        /// <summary>
        /// Retrieves information about a configuration action, based on its ID.
        /// </summary>
        /// <param name="actionId">The <see cref="ModerationActionEntity.Id"/> value of the configuration action to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested configuration action, or null if no such configuration action exists.
        /// </returns>
        Task<ConfigurationActionSummary> ReadAsync(long actionId);

        /// <summary>
        /// Updates data for an existing configuration action, based on its ID.
        /// </summary>
        /// <param name="actionId">The <see cref="ModerationActionEntity.Id"/> value of the configuration action to be updated.</param>
        /// <param name="updateAction">An action that will perform the desired update.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified configuration action could be found).
        /// </returns>
        Task<bool> UpdateAsync(long actionId, Action<ConfigurationActionMutationData> updateAction);
    }
}
