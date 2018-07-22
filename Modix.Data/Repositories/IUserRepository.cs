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
        /// Creates a new user within the repository, or updates an existing one.
        /// </summary>
        /// <param name="userId">The <see cref="UserEntity.Id"/> value of the user to be created or updated.</param>
        /// <param name="updateAction">An action that will perform the desired update upon the new or existing user.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>A <see cref="Task"/> which will complete when the operation is complete.</returns>
        Task CreateOrUpdateAsync(ulong userId, Action<UserMutationData> updateAction);

        /// <summary>
        /// Retrieves summary information about an existing user.
        /// </summary>
        /// <param name="userId">The <see cref="UserEntity.Id"/> value of the user to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested user summary information, or null if no such user exists.
        /// </returns>
        Task<UserSummary> ReadAsync(ulong userId);
    }
}
