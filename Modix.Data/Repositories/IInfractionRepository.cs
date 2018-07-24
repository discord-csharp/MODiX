using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="InfractionEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IInfractionRepository
    {
        /// <summary>
        /// Attempts to create a new infraction within the repository.
        /// </summary>
        /// <param name="data">The data for the infraction to be created.</param>
        /// <param name="criteria">A set of criteria that (if given) defines existing infractions that negate the need to create a new one.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="InfractionEntity.Id"/> value assigned to the new infraction,
        /// or null if any existing infractions were found by <paramref name="criteria"/>.
        /// </returns>
        Task<long?> TryCreateAsync(InfractionCreationData data, InfractionSearchCriteria criteria = null);

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
        /// Searches the repository for infraction ID values, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionEntity.Id"/> values to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the requested ID values have been retrieved.</returns>
        Task<IReadOnlyCollection<long>> SearchIdsAsync(InfractionSearchCriteria searchCriteria);

        /// <summary>
        /// Searches the repository for infraction information, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSummary"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<IReadOnlyCollection<InfractionSummary>> SearchSummariesAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria = null);

        /// <summary>
        /// Searches the repository for infraction information, based on an arbitrary set of criteria, and pages the results.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="InfractionSummary"/> records to be returned.</param>
        /// <param name="sortingCriteria">The criteria for sorting the matching records to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<RecordsPage<InfractionSummary>> SearchSummariesPagedAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria);

        /// <summary>
        /// Marks an existing infraction as rescinded, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be rescinded.</param>
        /// <param name="rescindedById">The <see cref="UserEntity.Id"/> value of the user that is rescinding the infraction.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified infraction could be found).
        /// </returns>
        Task<bool> TryRescindAsync(long infractionId, ulong rescindedById);

        /// <summary>
        /// Marks an existing infraction as deleted, based on its ID.
        /// </summary>
        /// <param name="infractionId">The <see cref="InfractionEntity.Id"/> value of the infraction to be deleted.</param>
        /// <param name="deletedById">The <see cref="UserEntity.Id"/> value of the user that is deleting the infraction.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the operation was successful (I.E. whether the specified infraction could be found).
        /// </returns>
        Task<bool> TryDeleteAsync(long infractionId, ulong deletedById);
    }
}
