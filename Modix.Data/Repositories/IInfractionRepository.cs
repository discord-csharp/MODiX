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
        /// Inserts a new infraction into the repository.
        /// </summary>
        /// <param name="infraction">The data for the infraction to be inserted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="infraction"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="InfractionEntity.Id"/> value assigned to the new infraction.
        /// </returns>
        Task<long> InsertAsync(InfractionData infraction);

        /// <summary>
        /// Checks whether an infraction exists within the repository.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to check for.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag that indicates if the infraction exists (true) or not (false).
        /// </returns>
        Task<bool> ExistsAsync(long infractionId);

        /// <summary>
        /// Retrieves information about an infraction, from its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested infraction, or null if no such infraction exists.
        /// </returns>
        Task<InfractionSummary> GetAsync(long infractionId);

        /// <summary>
        /// Updates the <see cref="InfractionEntity.RescindAction"/> value of an existing infraction
        /// to refer to an existing <see cref="ModerationActionEntity"/>.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be rescinded.</param>
        /// <param name="rescindActionId">the <see cref="ModerationActionEntity.Id"/> value of the moderation action that rescinded the infraction.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task SetRescindActionAsync(long infractionId, long rescindActionId);

        /// <summary>
        /// Searches the repository for infraction information, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSummary"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<IReadOnlyCollection<InfractionSummary>> SearchAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria);

        /// <summary>
        /// Searches the repository for infraction information, based on an arbitrary set of criteria, and pages the results.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSummary"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<RecordsPage<InfractionSummary>> SearchAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);
    }
}
