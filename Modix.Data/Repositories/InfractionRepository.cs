using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models.Admin;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="Infraction"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IInfractionRepository
    {
        /// <summary>
        /// Inserts a new <see cref="Infraction"/> into the repository.
        /// </summary>
        /// <param name="infraction">
        /// The <see cref="Infraction"/> to be inserted.
        /// The <see cref="Infraction.Id"/> and <see cref="Infraction.Created"/> values are generated automatically.
        /// </param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task InsertAsync(Infraction infraction);

        /// <summary>
        /// Updates the <see cref="Infraction.Duration"/> value of an existing <see cref="Infraction"/> within the repository.
        /// </summary>
        /// <param name="id">The <see cref="Infraction.Id"/> value of the <see cref="Infraction"/> to be updated.</param>
        /// <param name="duration">The new <see cref="Infraction.Duration"/> value to be saved into the repository.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task UpdateDurationAsync(ulong id, TimeSpan duration);

        /// <summary>
        /// Updates the <see cref="Infraction.IsRescinded"/> value of an existing <see cref="Infraction"/> within the repository.
        /// </summary>
        /// <param name="id">The <see cref="Infraction.Id"/> value of the <see cref="Infraction"/> to be updated.</param>
        /// <param name="isRescinded">The new <see cref="Infraction.IsRescinded"/> value to be saved into the repository.</param>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task UpdateIsRescindedAsync(ulong id, bool isRescinded);

        /// <summary>
        /// Searches the repository for <see cref="Infraction"/> entities, based on a given set of criteria.
        /// </summary>
        /// <param name="searchCriteria">The criteria for selecting <see cref="Infraction"/> entities to be returned.</param>
        /// <param name="pagingCriteria">The criteria for selecting a subset of matching entities to be returned.</param>
        /// <returns>A <see cref="Task"/> which will complete when the requested entities have been retrieved.</returns>
        Task<ICollection<Infraction>> SearchAsync(InfractionSearchCriteria searchCriteria, PagingCriteria pagingCriteria);
    }
}
