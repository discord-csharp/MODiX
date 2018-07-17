using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    /// <summary>
    /// Describes a repository for managing <see cref="RoleClaimEntity"/> entities, within an underlying data storage provider.
    /// </summary>
    public interface IRoleClaimRepository
    {
        /// <summary>
        /// Creates a new role claim mapping within the repository.
        /// </summary>
        /// <param name="data">The data for the mapping to be created.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="data"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the auto-generated <see cref="InfractionEntity.Id"/> value assigned to the new mapping.
        /// </returns>
        Task<long> CreateAsync(RoleClaimCreationData data);

        /// <summary>
        /// Checks whether a role claim mapping exists within the repository, based on its ID.
        /// </summary>
        /// <param name="roleClaimId">The <see cref="RoleClaimEntity.Id"/> value of the mapping to check for.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag that indicates if the mapping exists (true) or not (false).
        /// </returns>
        Task<bool> ExistsAsync(long roleClaimId);

        /// <summary>
        /// Checks whether an active role claim mapping with the given properties exists, within the repository.
        /// </summary>
        /// <param name="guildId">The <see cref="RoleClaimEntity.GuildId"/> value to check for.</param>
        /// <param name="roleId">The <see cref="RoleClaimEntity.RoleId"/> value to check for.</param>
        /// <param name="claim">The <see cref="RoleClaimEntity.Claim"/> value to check for.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag that indicates if the mapping exists (true) or not (false).
        /// </returns>
        Task<bool> ExistsAsync(ulong guildId, ulong roleId, AuthorizationClaim claim);

        /// <summary>
        /// Retrieves information about a role claim mapping, based on its ID.
        /// </summary>
        /// <param name="roleClaimId">The <see cref="RoleClaimEntity.Id"/> value of the mapping to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested mapping, or null if no such mapping exists.
        /// </returns>
        Task<RoleClaimSummary> ReadAsync(long roleClaimId);

        /// <summary>
        /// Retrieves a list of claims assigned to the given roles, within the given guild.
        /// </summary>
        /// <param name="guildId">The <see cref="RoleClaimEntity.GuildId"/> value of the claims to be retrieved.</param>
        /// <param name="roleIds">A set of <see cref="RoleClaimEntity.RoleId"/> values of the claims to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing the requested list of claims.
        /// </returns>
        Task<IReadOnlyCollection<AuthorizationClaim>> ReadClaimsAsync(ulong guildId, params ulong[] roleIds);

        /// <summary>
        /// Updates data for an existing role claim mapping, based on its ID.
        /// </summary>
        /// <param name="roleClaimId">The <see cref="RoleClaimEntity.Id"/> value of the mapping to be updated.</param>
        /// <param name="updateAction">An action that will perform the desired update.</param>
        /// A <see cref="Task"/> which will complete when the operation is complete,
        /// containing a flag indicating whether the update was successful (I.E. whether the specified mapping could be found).
        /// </returns>
        Task<bool> UpdateAsync(long roleClaimId, Action<RoleClaimMutationData> updateAction);
    }
}
