using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for managing and interacting with Discord roles, designated for use within the application.
    /// </summary>
    public interface IDesignatedRoleService
    {
        /// <summary>
        /// Assigns a role to a given designation, within a given guild.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild within which the designation is being made</param>
        /// <param name="roleId">The Discord snowflake ID of the role being designated</param>
        /// <param name="type">The type of designation to be made</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task<long> AddDesignatedRoleAsync(ulong guildId, ulong roleId, DesignatedRoleType type);

        /// <summary>
        /// Unassigns a role's previously given designation.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild for which a designation is being removed</param>
        /// <param name="roleId">The Discord snowflake ID of the role whose designation is being removed</param>
        /// <param name="type">The type of designation to be removed</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveDesignatedRoleAsync(ulong guildId, ulong roleId, DesignatedRoleType type);

        /// <summary>
        /// Unassigns a role designation by ID
        /// </summary>
        /// <param name="id">The ID of the assignment to remove</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveDesignatedRoleByIdAsync(long id);

        /// <summary>
        /// Retrieves the current designated roles, for a given guild.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild whose role designations are to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested role designations.
        /// </returns>
        Task<IReadOnlyCollection<DesignatedRoleMappingBrief>> GetDesignatedRolesAsync(ulong guildId);

        /// <summary>
        /// Retrieves designated roles, based on an arbitrary set of search criteria.
        /// </summary>
        /// <param name="searchCriteria">The search criteria, defining the role designations to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested role designations.
        /// </returns>
        Task<IReadOnlyCollection<DesignatedRoleMappingBrief>> SearchDesignatedRolesAsync(DesignatedRoleMappingSearchCriteria searchCriteria);

        /// <summary>
        /// Checks if the given role has the given designation
        /// </summary>
        /// <param name="guild">The Id of the guild where the role is located</param>
        /// <param name="roleId">The Id of the role to check the designation for</param>
        /// <param name="designation">The <see cref="DesignatedRoleType"/> to check for</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        Task<bool> RoleHasDesignationAsync(
            ulong guildId,
            ulong roleId,
            DesignatedRoleType designation,
            CancellationToken cancellationToken);

        /// <summary>
        /// Checks if the given set of roles has the given designation
        /// </summary>
        /// <param name="guild">The Id of the guild where the role is located</param>
        /// <param name="roleIds">The Id values of the roles to check the designation for</param>
        /// <param name="designation">The <see cref="DesignatedRoleType"/> to check for</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        Task<bool> RolesHaveDesignationAsync(
            ulong guildId,
            IReadOnlyCollection<ulong> roleIds,
            DesignatedRoleType designation,
            CancellationToken cancellationToken);
    }

    public class DesignatedRoleService : IDesignatedRoleService
    {
        public DesignatedRoleService(IAuthorizationService authorizationService, IDesignatedRoleMappingRepository designatedRoleMappingRepository)
        {
            AuthorizationService = authorizationService;
            DesignatedRoleMappingRepository = designatedRoleMappingRepository;
        }

        /// <inheritdoc />
        public async Task<long> AddDesignatedRoleAsync(ulong guildId, ulong roleId, DesignatedRoleType type)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingCreate);

            using (var transaction = await DesignatedRoleMappingRepository.BeginCreateTransactionAsync())
            {
                if (await DesignatedRoleMappingRepository.AnyAsync(new DesignatedRoleMappingSearchCriteria()
                {
                    GuildId = guildId,
                    RoleId = roleId,
                    Type = type,
                    IsDeleted = false
                }, default))
                    throw new InvalidOperationException($"Role {roleId} already has a {type} designation");

                var entityId = await DesignatedRoleMappingRepository.CreateAsync(new DesignatedRoleMappingCreationData()
                {
                    GuildId = guildId,
                    RoleId = roleId,
                    Type = type,
                    CreatedById = AuthorizationService.CurrentUserId.Value
                });

                transaction.Commit();

                return entityId;
            }
        }

        /// <inheritdoc />
        public async Task RemoveDesignatedRoleAsync(ulong guildId, ulong roleId, DesignatedRoleType type)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingDelete);

            using (var transaction = await DesignatedRoleMappingRepository.BeginDeleteTransactionAsync())
            {
                var deletedCount = await DesignatedRoleMappingRepository.DeleteAsync(new DesignatedRoleMappingSearchCriteria()
                {
                    GuildId = guildId,
                    RoleId = roleId,
                    Type = type,
                    IsDeleted = false
                }, AuthorizationService.CurrentUserId.Value);

                if (deletedCount == 0)
                    throw new InvalidOperationException($"Role {roleId} does not have a {type} designation");

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task RemoveDesignatedRoleByIdAsync(long id)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingDelete);

            using (var transaction = await DesignatedRoleMappingRepository.BeginDeleteTransactionAsync())
            {
                var deletedCount = await DesignatedRoleMappingRepository.DeleteAsync(new DesignatedRoleMappingSearchCriteria()
                {
                    Id = id,
                    IsDeleted = false
                }, AuthorizationService.CurrentUserId.Value);

                if (deletedCount == 0)
                    throw new InvalidOperationException($"No role assignment exists with id {id}");

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DesignatedRoleMappingBrief>> GetDesignatedRolesAsync(ulong guildId)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingRead);

            return DesignatedRoleMappingRepository.SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria()
            {
                GuildId = guildId,
                IsDeleted = false
            });
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DesignatedRoleMappingBrief>> SearchDesignatedRolesAsync(DesignatedRoleMappingSearchCriteria searchCriteria)
            => DesignatedRoleMappingRepository.SearchBriefsAsync(searchCriteria);

        public Task<bool> RoleHasDesignationAsync(
                ulong guildId,
                ulong roleId,
                DesignatedRoleType designation,
                CancellationToken cancellationToken)
            => DesignatedRoleMappingRepository.AnyAsync(new DesignatedRoleMappingSearchCriteria
            {
                GuildId = guildId,
                RoleId = roleId,
                IsDeleted = false,
                Type = designation
            }, default);

        public Task<bool> RolesHaveDesignationAsync(
                ulong guildId,
                IReadOnlyCollection<ulong> roleIds,
                DesignatedRoleType designation,
                CancellationToken cancellationToken)
            => DesignatedRoleMappingRepository.AnyAsync(new DesignatedRoleMappingSearchCriteria()
            {
                GuildId = guildId,
                RoleIds = roleIds.ToArray(),
                IsDeleted = false,
                Type = designation
            }, default);

        /// <summary>
        /// A <see cref="IAuthorizationService"/> to be used to perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IDesignatedRoleMappingRepository"/> for storing and retrieving role designation data.
        /// </summary>
        internal protected IDesignatedRoleMappingRepository DesignatedRoleMappingRepository { get; }
    }
}
