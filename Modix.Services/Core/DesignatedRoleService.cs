using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Serilog;

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
        Task AddDesignatedRoleAsync(ulong guildId, ulong roleId, DesignatedRoleType type);

        /// <summary>
        /// Unassigns a role's previously given designation.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild for which a designation is being removed</param>
        /// <param name="roleId">The Discord snowflake ID of the role whose designation is being removed</param>
        /// <param name="type">The type of designation to be removed</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveDesignatedRoleAsync(ulong guildId, ulong roleId, DesignatedRoleType type);

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
    }

    public class DesignatedRoleService : IDesignatedRoleService
    {
        public DesignatedRoleService(IAuthorizationService authorizationService, IDesignatedRoleMappingRepository designatedRoleMappingRepository)
        {
            AuthorizationService = authorizationService;
            DesignatedRoleMappingRepository = designatedRoleMappingRepository;
        }

        /// <inheritdoc />
        public async Task AddDesignatedRoleAsync(ulong guildId, ulong roleId, DesignatedRoleType type)
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
                }))
                    throw new InvalidOperationException($"Role {roleId} already has a {type} designation");

                await DesignatedRoleMappingRepository.CreateAsync(new DesignatedRoleMappingCreationData()
                {
                    GuildId = guildId,
                    RoleId = roleId,
                    Type = type,
                    CreatedById = AuthorizationService.CurrentUserId.Value
                });

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task RemoveDesignatedRoleAsync(ulong guildId, ulong roleId, DesignatedRoleType type)
        {
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.DesignatedRoleMappingCreate);

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
