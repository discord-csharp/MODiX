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
        /// Inserts a new <see cref="ModerationActionEntity"/> into the repository.
        /// </summary>
        /// <param name="action">
        /// The <see cref="ModerationActionEntity"/> to be inserted.
        /// The <see cref="ModerationActionEntity.Id"/> and <see cref="ModerationActionEntity.Created"/> values are generated automatically.
        /// </param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="action"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="ModerationActionEntity.Id"/> value assigned to <paramref name="action"/>.
        /// </returns>
        Task<long> InsertAsync(ModerationActionEntity action);

        /// <summary>
        /// Updates the <see cref="ModerationActionEntity.Infraction"/> value of an existing moderation action,
        /// to refer to an existing <see cref="InfractionEntity"/>.
        /// </summary>
        /// <param name="actionId">The <see cref="ModerationActionEntity.Id"/> value of the action to be updated.</param>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction that is related to the moderation action.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task SetInfractionAsync(long actionId, long infractionId);
    }
}
