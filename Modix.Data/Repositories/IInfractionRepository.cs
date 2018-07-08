using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models;
using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="InfractionEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IInfractionRepository
    {
        /// <summary>
        /// Inserts a new <see cref="InfractionEntity"/> into the repository.
        /// </summary>
        /// <param name="infraction">
        /// The <see cref="InfractionEntity"/> to be inserted.
        /// The <see cref="InfractionEntity.Id"/> value is generated automatically.
        /// </param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="infraction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="InfractionEntity.Id"/> value assigned to <paramref name="infraction"/>.
        /// </returns>
        Task<long> InsertAsync(InfractionEntity infraction);

        /// <summary>
        /// Updates the <see cref="InfractionEntity.RescindAction"/> value of an existing infraction
        /// to refer to an existing <see cref="ModerationActionEntity"/>.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be rescinded.</param>
        /// <param name="rescindActionId">the <see cref="ModerationActionEntity.Id"/> value of the moderation action that rescinded the infraction.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task SetRescindActionAsync(long infractionId, long rescindActionId);

        /// <summary>
        /// Searches the repository for infractions, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSearchResult"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<IReadOnlyCollection<InfractionSearchResult>> SearchAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria);

        /// <summary>
        /// Searches the repository for infractions, based on an arbitrary set of criteria, and pages the results.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSearchResult"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<RecordsPage<InfractionSearchResult>> SearchAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);
    }
}
