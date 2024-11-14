#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services.Utilities;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for authorizing an action to be performed, within the context of a scoped request.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// The unique identifier, within the Discord API, of the authenticated user (if any) that generated the current request.
        /// </summary>
        ulong? CurrentUserId { get; }

        /// <summary>
        /// The unique identifier, within the Discord API, of the guild (if any) form which the current request was generated.
        /// </summary>
        ulong? CurrentGuildId { get; }

        /// <summary>
        /// Modifies a claim mapping for a role.
        /// </summary>
        /// <param name="roleId">The role for which the claim mapping is to be modified.</param>
        /// <param name="claim">The claim to be mapped.</param>
        /// <param name="newType">The type to modify the claim mapping to. If null, the mapping will be removed.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task ModifyClaimMappingAsync(ulong roleId, AuthorizationClaim claim, ClaimMappingType? newType);

        /// <summary>
        /// Adds a claim mapping to a role.
        /// </summary>
        /// <param name="role">The role for which a claim mapping is to be added.</param>
        /// <param name="type">The type of claim mapping to be added.</param>
        /// <param name="claim">The claim to be mapped.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AddClaimMappingAsync(IRole role, ClaimMappingType type, AuthorizationClaim claim);

        /// <summary>
        /// Adds a claim mapping to a user.
        /// </summary>
        /// <param name="user">The user for which a claim mapping is to be added.</param>
        /// <param name="type">The type of claim mapping to be added.</param>
        /// <param name="claim">The claim to be mapped.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AddClaimMappingAsync(IGuildUser user, ClaimMappingType type, AuthorizationClaim claim);

        /// <summary>
        /// Removes a claim mapping from a role.
        /// </summary>
        /// <param name="role">The role for which a claim mapping is to be removed.</param>
        /// <param name="type">The type of claim mapping to be removed.</param>
        /// <param name="claim">The claim to be un-mapped.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveClaimMappingAsync(IRole role, ClaimMappingType type, AuthorizationClaim claim);

        /// <summary>
        /// Removes a claim mapping from a user.
        /// </summary>
        /// <param name="role">The user for which a claim mapping is to be removed.</param>
        /// <param name="type">The type of claim mapping to be removed.</param>
        /// <param name="claim">The claim to be un=mapped.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveClaimMappingAsync(IGuildUser user, ClaimMappingType type, AuthorizationClaim claim);

        /// <summary>
        /// A list of authorization claims possessed by the source of the current request.
        /// </summary>
        IReadOnlyCollection<AuthorizationClaim> CurrentClaims { get; }

        /// <summary>
        /// Retrieves the list of claims currently active and mapped to particular user, within a particular guild.
        /// </summary>
        /// <param name="guildUser">The user whose claims are to be retrieved.</param>
        /// <param name="claimsFilter">
        /// An optional list of claims to be used to filter the results.
        /// I.E. the returned list of claims will only contain claims specified in this list (unless none are specified).
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested list of claims.
        /// </returns>
        Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserClaimsAsync(IGuildUser guildUser, params AuthorizationClaim[] claimsFilter);

        /// <summary>
        /// Retrieves the list of claims currently active and mapped to particular user, within a particular guild.
        /// </summary>
        /// <param name="user">The user whose claims are to be retrieved.</param>
        /// <param name="guild"></param>
        /// <param name="roles"></param>
        /// <param name="filterClaims">
        /// An optional list of claims to be used to filter the results.
        /// I.E. the returned list of claims will only contain claims specified in this list (unless none are specified).
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested list of claims.
        /// </returns>
        public Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserClaimsAsync(ulong user, ulong guild, IReadOnlyList<ulong> roles, params AuthorizationClaim[] filterClaims);

        /// <summary>
        /// Retrieves the list of claims currently active and mapped to particular role.
        /// </summary>
        /// <param name="guildRole">The role whose claims are to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested list of claims.
        /// </returns>
        Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildRoleClaimsAsync(IRole guildRole);

        /// <summary>
        /// Compares a given set of claims against the full set of claims posessed by a given user,
        /// to determine which claims, if any, are missing.
        /// </summary>
        /// <param name="user">The user whose claims are to be checked.</param>
        /// <param name="guild"></param>
        /// <param name="roles"></param>
        /// <param name="claims">The set of claims to be compared against the claims posessed by <paramref name="guildUser"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the set of claims present in <paramref name="claims"/>, but not posessed by <paramref name="guildUser"/>.
        /// </returns>
        Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserMissingClaimsAsync(ulong user, ulong guild, IReadOnlyList<ulong> roles, params AuthorizationClaim[] claims);

        /// <summary>
        /// Checks whether a given user currently posesses a set of claims.
        /// </summary>
        /// <param name="user">The user whose claims are to be checked.</param>
        /// <param name="guild"></param>
        /// <param name="roles"></param>
        /// <param name="claims">The set of claims to be checked for.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether <paramref name="guildUser"/> posesses <paramref name="claims"/>.
        /// </returns>
        Task<bool> HasClaimsAsync(ulong user, ulong guild, IReadOnlyList<ulong>? roles, params AuthorizationClaim[] claims);

        /// <summary>
        /// Loads authentication and authorization data into the service, based on the given guild, user, and role ID values
        /// retrieved from a frontend authentication mechanism.
        /// </summary>
        /// <param name="user">The user to be authenticated</param>
        /// <param name="guild"></param>
        /// <param name="roles"></param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task OnAuthenticatedAsync(ulong user, ulong guild, IReadOnlyList<ulong> roles);

        /// <summary>
        /// Loads and authentication and authorization data into the service, for self-initiated operations.
        /// Self-authentication is basically a way to have self-initiated bot actions reuse code that is exposed to consumers,
        /// by having the bot user itself be registered as <see cref="CurrentUserId"/>, with no <see cref="CurrentGuildId"/>,
        /// and with a <see cref="CurrentClaims"/> collection that contains every claim that exists for the application.
        /// </summary>
        /// <param name="self">The current Discord self-user.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task OnAuthenticatedAsync(ISelfUser self);

#nullable enable
        /// <summary>
        /// Requires that there be an authenticated guild for the current request.
        /// </summary>
        [MemberNotNull(nameof(CurrentGuildId))]
        void RequireAuthenticatedGuild();

        /// <summary>
        /// Requires that there be an authenticated user for the current request.
        /// </summary>
        [MemberNotNull(nameof(CurrentUserId))]
        void RequireAuthenticatedUser();
#nullable restore

        /// <summary>
        /// Requires that the given set of claims be present, for the current request.
        /// </summary>
        /// <param name="claims">A set of claims to be checked against <see cref="CurrentClaims"/>.</param>
        void RequireClaims(params AuthorizationClaim[] claims);

        bool HasClaim(AuthorizationClaim claim);
    }

    /// <inheritdoc />
    public class AuthorizationService : IAuthorizationService
    {
        public AuthorizationService(DiscordSocketClient discordSocketClient, IServiceProvider serviceProvider, IClaimMappingRepository claimMappingRepository)
        {
            // Workaround for circular dependency.
            _lazyUserService = new Lazy<IUserService>(() => serviceProvider.GetRequiredService<IUserService>());

            ClaimMappingRepository = claimMappingRepository;
            _discordSocketClient = discordSocketClient;
        }

        /// <inheritdoc />
        public ulong? CurrentUserId { get; internal protected set; }

        /// <inheritdoc />
        public ulong? CurrentGuildId { get; internal protected set; }

        /// <inheritdoc />
        public IReadOnlyCollection<AuthorizationClaim> CurrentClaims { get; internal protected set; }

        /// <inheritdoc />
        public async Task ModifyClaimMappingAsync(ulong roleId, AuthorizationClaim claim, ClaimMappingType? newType)
        {
            RequireAuthenticatedUser();
            RequireClaims(AuthorizationClaim.AuthorizationConfigure);

            var foundClaims = await ClaimMappingRepository.SearchBriefsAsync(new ClaimMappingSearchCriteria
            {
                Claims = new[] { claim },
                RoleIds = new[] { roleId },
                GuildId = CurrentGuildId
            });

            using var createTransaction = await ClaimMappingRepository.BeginCreateTransactionAsync();
            using var deleteTransaction = await ClaimMappingRepository.BeginDeleteTransactionAsync();

            foreach (var existing in foundClaims)
            {
                await ClaimMappingRepository.TryDeleteAsync(existing.Id, CurrentUserId.Value);
            }

            if (newType.HasValue)
            {
                await ClaimMappingRepository.CreateAsync(new ClaimMappingCreationData()
                {
                    GuildId = CurrentGuildId.Value,
                    Type = newType.Value,
                    RoleId = roleId,
                    Claim = claim,
                    CreatedById = CurrentUserId.Value
                });
            }

            createTransaction.Commit();
            deleteTransaction.Commit();
        }

        /// <inheritdoc />
        public async Task AddClaimMappingAsync(IRole role, ClaimMappingType type, AuthorizationClaim claim)
        {
            RequireAuthenticatedUser();
            RequireClaims(AuthorizationClaim.AuthorizationConfigure);

            using var transaction = await ClaimMappingRepository.BeginCreateTransactionAsync();

            if (await ClaimMappingRepository.AnyAsync(new ClaimMappingSearchCriteria()
            {
                Types = new[] { type },
                GuildId = role.Guild.Id,
                RoleIds = new[] { role.Id },
                Claims = new[] { claim },
                IsDeleted = false,
            }))
            {
                throw new InvalidOperationException($"A claim mapping of type {type} to claim {claim} for role {role.Name} already exists");
            }

            await ClaimMappingRepository.CreateAsync(new ClaimMappingCreationData()
            {
                GuildId = role.Guild.Id,
                Type = type,
                RoleId = role.Id,
                Claim = claim,
                CreatedById = CurrentUserId.Value
            });

            transaction.Commit();
        }

        /// <inheritdoc />
        public async Task AddClaimMappingAsync(IGuildUser user, ClaimMappingType type, AuthorizationClaim claim)
        {
            RequireAuthenticatedUser();
            RequireClaims(AuthorizationClaim.AuthorizationConfigure);

            using var transaction = await ClaimMappingRepository.BeginCreateTransactionAsync();

            if (await ClaimMappingRepository.AnyAsync(new ClaimMappingSearchCriteria()
            {
                Types = new[] { type },
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Claims = new[] { claim },
                IsDeleted = false,
            }))
            {
                throw new InvalidOperationException($"A claim mapping of type {type} to claim {claim} for user {user.GetDisplayName()} already exists");
            }

            await ClaimMappingRepository.CreateAsync(new ClaimMappingCreationData()
            {
                GuildId = user.Guild.Id,
                Type = type,
                UserId = user.Id,
                Claim = claim,
                CreatedById = CurrentUserId.Value
            });

            transaction.Commit();
        }

        /// <inheritdoc />
        public async Task RemoveClaimMappingAsync(IRole role, ClaimMappingType type, AuthorizationClaim claim)
        {
            RequireAuthenticatedUser();
            RequireClaims(AuthorizationClaim.AuthorizationConfigure);

            using var transaction = await ClaimMappingRepository.BeginDeleteTransactionAsync();

            var mappingIds = await ClaimMappingRepository.SearchIdsAsync(new ClaimMappingSearchCriteria()
            {
                Types = new[] { type },
                GuildId = role.Guild.Id,
                RoleIds = new[] { role.Id },
                Claims = new[] { claim },
                IsDeleted = false,
            });

            if (!mappingIds.Any())
                throw new InvalidOperationException($"A claim mapping of type {type} to claim {claim} for role {role.Name} does not exist");

            await ClaimMappingRepository.TryDeleteAsync(mappingIds.First(), CurrentUserId.Value);

            transaction.Commit();
        }

        /// <inheritdoc />
        public async Task RemoveClaimMappingAsync(IGuildUser user, ClaimMappingType type, AuthorizationClaim claim)
        {
            RequireAuthenticatedUser();
            RequireClaims(AuthorizationClaim.AuthorizationConfigure);

            using var transaction = await ClaimMappingRepository.BeginDeleteTransactionAsync();

            var mappingIds = await ClaimMappingRepository.SearchIdsAsync(new ClaimMappingSearchCriteria()
            {
                Types = new[] { type },
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Claims = new[] { claim },
                IsDeleted = false,
            });

            if (!mappingIds.Any())
                throw new InvalidOperationException($"A claim mapping of type {type} to claim {claim} for user {user.GetDisplayName()} does not exist");

            await ClaimMappingRepository.TryDeleteAsync(mappingIds.First(), CurrentUserId.Value);

            transaction.Commit();
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserClaimsAsync(IGuildUser guildUser, params AuthorizationClaim[] filterClaims)
        {
            if (guildUser == null)
                return Task.FromException<IReadOnlyCollection<AuthorizationClaim>>(new ArgumentNullException(nameof(guildUser)));

            if (guildUser.Id == CurrentUserId)
                return Task.FromResult(CurrentClaims);

            if ((guildUser.Id == _discordSocketClient.CurrentUser.Id) || guildUser.GuildPermissions.Administrator)
                return Task.FromResult<IReadOnlyCollection<AuthorizationClaim>>(Enum.GetValues<AuthorizationClaim>());

            return LookupPosessedClaimsAsync(guildUser.GuildId, guildUser.RoleIds, guildUser.Id, filterClaims);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserClaimsAsync(ulong userId, ulong guildId, IReadOnlyList<ulong> roles, params AuthorizationClaim[] filterClaims)
        {
            if (userId == default)
                throw new ArgumentNullException(nameof(userId));

            if (userId == CurrentUserId)
                return CurrentClaims;

            if (userId == _discordSocketClient.CurrentUser.Id)
                return Enum.GetValues<AuthorizationClaim>();

            var guild = _discordSocketClient.GetGuild(guildId);
            var user = guild.GetUser(userId);

            if (user.GuildPermissions.Administrator)
                return Enum.GetValues<AuthorizationClaim>();

            return await LookupPosessedClaimsAsync(guildId, roles, userId, filterClaims);
        }
        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildRoleClaimsAsync(IRole guildRole)
        {
            return (await ClaimMappingRepository.SearchBriefsAsync(new ClaimMappingSearchCriteria
            {
                RoleIds = new[] { guildRole.Id },
                IsDeleted = false,
                Types = new[] { ClaimMappingType.Granted },
                GuildId = guildRole.Guild.Id
            }))
            .Select(d=>d.Claim)
            .ToList();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserMissingClaimsAsync(ulong user, ulong guild, IReadOnlyList<ulong> roles, params AuthorizationClaim[] claims)
            => claims.Except(await GetGuildUserClaimsAsync(user, guild, roles, claims))
                .ToArray();

        /// <inheritdoc />
        public async Task<bool> HasClaimsAsync(ulong user, ulong guild, IReadOnlyList<ulong> roles, params AuthorizationClaim[] claims)
            => !(await GetGuildUserMissingClaimsAsync(user, guild, roles, claims)).Any();

        /// <inheritdoc />
        public async Task OnAuthenticatedAsync(ulong user, ulong guild, IReadOnlyList<ulong> roles)
        {
            CurrentClaims = await GetGuildUserClaimsAsync(user, guild, roles);
            CurrentGuildId = guild;
            CurrentUserId = user;
        }

        /// <inheritdoc />
        public Task OnAuthenticatedAsync(ISelfUser self)
        {
            CurrentGuildId = null;
            CurrentUserId = self.Id;
            CurrentClaims = Enum.GetValues(typeof(AuthorizationClaim))
                .Cast<AuthorizationClaim>()
                .ToHashSet();

            return Task.CompletedTask;
        }

#nullable enable
        /// <inheritdoc />
        [MemberNotNull(nameof(CurrentGuildId))]
        public void RequireAuthenticatedGuild()
        {
            if (CurrentGuildId == null)
                // TODO: Booooo for exception-based flow control
                throw new InvalidOperationException("The current operation requires an authenticated guild.");
        }

        /// <inheritdoc />
        [MemberNotNull(nameof(CurrentUserId))]
        public void RequireAuthenticatedUser()
        {
            if (CurrentUserId == null)
                // TODO: Booooo for exception-based flow control
                throw new InvalidOperationException("The current operation requires an authenticated user.");
        }
#nullable restore

        /// <inheritdoc />
        public void RequireClaims(params AuthorizationClaim[] claims)
        {
            RequireAuthenticatedUser();

            if (claims == null)
                throw new ArgumentNullException(nameof(claims));

            var missingClaims = claims
                .Except(CurrentClaims)
                .ToArray();

            if (missingClaims.Length != 0)
                // TODO: Booooo for exception-based flow control
                throw new InvalidOperationException($"The current operation could not be authorized. The following claims were missing: {string.Join(", ", missingClaims)}");
        }

        public bool HasClaim(AuthorizationClaim claim)
        {
            RequireAuthenticatedUser();
            return CurrentClaims.Contains(claim);
        }

        /// <summary>
        /// An <see cref="IUserService"/> for interacting with discord users within the application.
        /// </summary>
        internal protected IUserService UserService
            => _lazyUserService.Value;
        // Workaround for circular dependency.
        private readonly Lazy<IUserService> _lazyUserService;
        private readonly DiscordSocketClient _discordSocketClient;

        /// <summary>
        /// An <see cref="IClaimMappingRepository"/> for storing and retrieving claim mapping data.
        /// </summary>
        internal protected IClaimMappingRepository ClaimMappingRepository { get; }

        private async Task<IReadOnlyCollection<AuthorizationClaim>> LookupPosessedClaimsAsync(ulong guildId, IEnumerable<ulong> roleIds, ulong userId, IEnumerable<AuthorizationClaim> claimsFilter = null)
        {
            var posessedClaims = new HashSet<AuthorizationClaim>();

            foreach (var claimMapping in (await ClaimMappingRepository
                .SearchBriefsAsync(new ClaimMappingSearchCriteria()
                {
                    GuildId = guildId,
                    RoleIds = roleIds.ToArray(),
                    UserId = userId,
                    Claims = claimsFilter?.ToArray(),
                    IsDeleted = false
                }))
                // Evaluate role mappings (userId is null) first, to give user mappings precedence.
                .OrderBy(x => x.UserId)
                // Order putting @everyone last
                .ThenByDescending(x => x.RoleId == guildId)
                // Evaluate granted mappings first, to give denied mappings precedence.
                .ThenBy(x => x.Type))
            {
                if (claimMapping.Type == ClaimMappingType.Granted)
                    posessedClaims.Add(claimMapping.Claim);
                else
                    posessedClaims.Remove(claimMapping.Claim);
            }

            return posessedClaims;
        }
    }
}
