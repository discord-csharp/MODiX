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
        /// Begins a new transaction to create users within the repository.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that will complete, with the requested transaction object,
        /// when no other transactions are active upon the repository.
        /// </returns>
        Task<IRepositoryTransaction> BeginCreateTransactionAsync();

        Task CreateAsync(UserCreationData data);

        /// <summary>
        /// Retrieves summary information about an existing user.
        /// </summary>
        /// <param name="userId">The <see cref="UserEntity.Id"/> value of the user to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested user summary information, or null if no such user exists.
        /// </returns>
        Task<UserSummary> ReadAsync(ulong userId);

        Task<bool> TryUpdateAsync(ulong userId, Action<UserMutationData> updateAction);
    }
}
