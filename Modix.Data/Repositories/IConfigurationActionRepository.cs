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
        /// Retrieves information about a configuration action, based on its ID.
        /// </summary>
        /// <param name="actionId">The <see cref="ModerationActionEntity.Id"/> value of the configuration action to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested configuration action, or null if no such configuration action exists.
        /// </returns>
        Task<ConfigurationActionSummary> ReadAsync(long actionId);
    }
}
