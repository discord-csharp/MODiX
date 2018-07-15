using System;
using System.Threading.Tasks;

using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="UserEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Creates a new user within the repository.
        /// </summary>
        /// <param name="data">The data for the user to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete</returns>
        Task CreateAsync(UserCreationData data);

        /// <summary>
        /// Checks whether a user exists within the repository, based on its ID.
        /// </summary>
        /// <param name="userId">The <see cref="UserEntity.Id"/> value of the user to check for.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag that indicates if the infraction exists (true) or not (false).
        /// </returns>
        Task<bool> ExistsAsync(ulong userId);

        /// <summary>
        /// Updates data for an existing user, based on its ID.
        /// </summary>
        /// <param name="userId">The <see cref="UserEntity.Id"/> value of the infraction to be updated.</param>
        /// <param name="updateAction">An action that will perform the desired update.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified user could be found).
        /// </returns>
        Task<bool> UpdateAsync(ulong userId, Action<UserMutationData> updateAction);
    }
}
