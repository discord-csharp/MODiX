using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models.Admin;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="ModerationAction"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IModerationActionRepository
    {
        /// <summary>
        /// Inserts a new <see cref="ModerationAction"/> into the repository.
        /// </summary>
        /// <param name="action">
        /// The <see cref="ModerationAction"/> to be inserted.
        /// The <see cref="ModerationAction.Id"/> and <see cref="ModerationAction.Created"/> values are generated automatically.
        /// </param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task InsertAsync(ModerationAction action);

        /// <summary>
        /// Searches the repository for <see cref="ModerationAction"/> entities, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="ModerationAction"/> entities to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching entities to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the requested entities have been retrieved.</returns>
        Task<ICollection<ModerationAction>> SearchAsync(ModerationActionSearchCriteria searchCriteria, PagingCriteria pagingCriteria);
    }
}
