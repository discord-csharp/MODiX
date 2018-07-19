using System;
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
        /// Creates a new infraction within the repository.
        /// </summary>
        /// <param name="data">The data for the infraction to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="InfractionEntity.Id"/> value assigned to the new infraction.
        /// </returns>
        Task<long> CreateAsync(InfractionCreationData data);

        /// <summary>
        /// Retrieves information about an infraction, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested infraction, or null if no such infraction exists.
        /// </returns>
        Task<InfractionSummary> ReadAsync(long infractionId);

        /// <summary>
        /// Searches the repository for infraction information, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSummary"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<IReadOnlyCollection<InfractionSummary>> SearchSummariesAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria);

        /// <summary>
        /// Searches the repository for infraction information, based on an arbitrary set of criteria, and pages the results.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSummary"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<RecordsPage<InfractionSummary>> SearchSummariesPagedAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);

        /// <summary>
        /// Updates data for an existing infraction, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be updated.</param>
        /// <param name="updateAction">An action that will perform the desired update.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified infraction could be found).
        /// </returns>
        Task<bool> UpdateAsync(long infractionId, Action<InfractionMutationData> updateAction);
    }
}
