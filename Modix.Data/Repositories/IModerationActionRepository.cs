using System;
using System.Threading.Tasks;

using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="ModerationActionEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IModerationActionRepository
    {
        /// <summary>
        /// Retrieves information about a moderation action, based on its ID.
        /// </summary>
        /// <param name="actionId">The <see cref="ModerationActionEntity.Id"/> value of the moderation action to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested moderation action, or null if no such moderation action exists.
        /// </returns>
        Task<ModerationActionSummary> ReadAsync(long actionId);
    }
}
