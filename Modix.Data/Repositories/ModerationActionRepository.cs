using System.Collections.Generic;
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
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="ModerationActionEntity.Id"/> value assigned to <paramref name="action"/>.
        /// </returns>
        Task<long> InsertAsync(ModerationActionEntity action);

        /// <summary>
        /// Searches the repository for <see cref="ModerationActionEntity"/> entities, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="ModerationActionEntity"/> entities to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching entities to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the requested entities have been retrieved.</returns>
        Task<ICollection<ModerationActionEntity>> SearchAsync(ModerationActionSearchCriteria searchCriteria, PagingCriteria pagingCriteria);
    }
}
