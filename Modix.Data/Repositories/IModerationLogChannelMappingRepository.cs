using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="ModerationLogChannelMappingEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IModerationLogChannelMappingRepository
    {
        /// <summary>
        /// Begins a new transaction to create mappings within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other create transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        /// <summary>
        /// Begins a new transaction to delete mappings within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other delete transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginDeleteTransactionAsync();

        /// <summary>
        /// Creates a new mapping withn the repository.
        /// This method must not be called outside of the scope of an <see cref="IRepositoryTransaction"/> returned by <see cref="BeginCreateTransactionAsync"/>.
        /// </summary>
        /// <param name="data">The data for the mapping to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="ModerationLogChannelMappingEntity.Id"/> value assigned to the new mapping.
        /// </returns>
        Task<long> CreateAsync(ModerationLogChannelMappingCreationData data);

        /// <summary>
        /// Checks whether any mappings exist within the repository, according to an arbitrary set of criteria.
        /// </summary>
        /// <param name="criteria">A set of criteria defining the mappings to check for.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether any matching log channel mappings were found.
        /// </returns>
        Task<bool> AnyAsync(ModerationLogChannelMappingSearchCriteria criteria);

        /// <summary>
        /// Searches the repository for mappings, based on an arbitrary set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="ModerationLogChannelMappingBrief"/> records to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the matching records have been retrieved.</returns>
        Task<IReadOnlyCollection<ModerationLogChannelMappingBrief>> SearchBriefsAsync(ModerationLogChannelMappingSearchCriteria searchCriteria);

        /// <summary>
        /// Marks mappings within the repository as deleted, based on a given set of search criteria.
        /// This method must not be called outside of the scope of an <see cref="IRepositoryTransaction"/> returned by <see cref="BeginDeleteTransactionAsync"/>.
        /// </summary>
        /// <param name="criteria">A set of criteria defining the mappings to be deleted.</param>
        /// <param name="deletedById">The <see cref="UserEntity.Id"/> value of the user that is deleting the mapping.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the total number of mappings that were deleted, based on the given criteria.
        /// </returns>
        Task<int> DeleteAsync(ModerationLogChannelMappingSearchCriteria criteria, ulong deletedById);

        /// <summary>
        /// Marks an existing mapping as deleted, based on its ID.
        /// This method must not be called outside of the scope of an <see cref="IRepositoryTransaction"/> returned by <see cref="BeginDeleteTransactionAsync"/>.
        /// </summary>
        /// <param name="mappingId">The <see cref="ModerationLogChannelMappingEntity.Id"/> value of the mapping to be deleted.</param>
        /// <param name="deletedById">The <see cref="UserEntity.Id"/> value of the user that is deleting the mapping.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the operation was successful (I.E. whether the specified mapping could be found).
        /// </returns>
        Task<bool> TryDeleteAsync(long mappingId, ulong deletedById);
    }
}
