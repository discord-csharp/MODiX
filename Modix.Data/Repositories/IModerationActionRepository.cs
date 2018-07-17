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
        /// Creates a new moderation action within the repository.
        /// </summary>
        /// <param name="data">The data for the moderation action to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="ModerationActionEntity.Id"/> value assigned to the new moderation action.
        /// </returns>
        Task<long> CreateAsync(ModerationActionCreationData data);

        /// <summary>
        /// Checks whether a moderation action exists, based on its ID.
        /// </summary>
        /// <param name="actionId">The <see cref="ModerationActionEntity.Id"/> value of the moderation action to check for.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether or not the moderation action exists.
        /// </returns>
        Task<bool> ExistsAsync(long actionId);

        /// <summary>
        /// Retrieves information about a moderation action, based on its ID.
        /// </summary>
        /// <param name="actionId">The <see cref="ModerationActionEntity.Id"/> value of the moderation action to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested moderation action, or null if no such moderation action exists.
        /// </returns>
        Task<ModerationActionSummary> ReadAsync(long actionId);

        /// <summary>
        /// Updates data for an existing moderation action, based on its ID.
        /// </summary>
        /// <param name="actionId">The <see cref="ModerationActionEntity.Id"/> value of the moderation action to be updated.</param>
        /// <param name="updateAction">An action that will perform the desired update.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified moderation action could be found).
        /// </returns>
        Task<bool> UpdateAsync(long actionId, Action<ModerationActionMutationData> updateAction);

    }
}
