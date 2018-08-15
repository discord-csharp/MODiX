using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;

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
        /// Automatically configures default claim mappings for a guild, if none yet exist.
        /// Default claims include granting all existing claims to any role that has the Discord "Administrate" permission.
        /// </summary>
        /// <param name="guild">The guild to be configured.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AutoConfigureGuildAsync(IGuild guild);

        /// <summary>
        /// Removes all authorization configuration for a guild, by rescinding all of its claim mappings.
        /// </summary>
        /// <param name="guild">The guild to be un-configured.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task UnConfigureGuildAsync(IGuild guild);

        /// <summary>
        /// Adds a claim mapping to a role.
        /// </summary>
        /// <param name="role">The role for which a claim mapping is to be added.</param>
        /// <param name="type">The type of claim mapping to be added.</param>
        /// <param name="claim">The claim to be mapped.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AddClaimMapping(IRole role, ClaimMappingType type, AuthorizationClaim claim);

        /// <summary>
        /// Adds a claim mapping to a user.
        /// </summary>
        /// <param name="user">The user for which a claim mapping is to be added.</param>
        /// <param name="type">The type of claim mapping to be added.</param>
        /// <param name="claim">The claim to be mapped.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AddClaimMapping(IGuildUser user, ClaimMappingType type, AuthorizationClaim claim);

        /// <summary>
        /// Removes a claim mapping from a role.
        /// </summary>
        /// <param name="role">The role for which a claim mapping is to be removed.</param>
        /// <param name="type">The type of claim mapping to be removed.</param>
        /// <param name="claim">The claim to be un-mapped.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveClaimMapping(IRole role, ClaimMappingType type, AuthorizationClaim claim);

        /// <summary>
        /// Removes a claim mapping from a user.
        /// </summary>
        /// <param name="role">The user for which a claim mapping is to be removed.</param>
        /// <param name="type">The type of claim mapping to be removed.</param>
        /// <param name="claim">The claim to be un=mapped.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task RemoveClaimMapping(IGuildUser user, ClaimMappingType type, AuthorizationClaim claim);

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
        /// Compares a given set of claims against the full set of claims posessed by a given user,
        /// to determine which claims, if any, are missing.
        /// </summary>
        /// <param name="guildUser">The user whose claims are to be checked.</param>
        /// <param name="claims">The set of claims to be compared against the claims posessed by <paramref name="guildUser"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the set of claims present in <paramref name="claims"/>, but not posessed by <paramref name="guildUser"/>.
        /// </returns>
        Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserMissingClaimsAsync(IGuildUser guildUser, params AuthorizationClaim[] claims);

        /// <summary>
        /// Checks whether a given user currently posesses a set of claims.
        /// </summary>
        /// <param name="guildUser">The user whose claims are to be checked.</param>
        /// <param name="claims">The set of claims to be checked for.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing a flag indicating whether <paramref name="guildUser"/> posesses <paramref name="claims"/>.
        /// </returns>
        Task<bool> HasClaimsAsync(IGuildUser guildUser, params AuthorizationClaim[] claims);

        /// <summary>
        /// Loads authentication and authorization data into the service, based on the given guild, user, and role ID values
        /// retrieved from a frontend authentication mechanism.
        /// </summary>
        /// <param name="user">The user to be authenticated</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task OnAuthenticatedAsync(IGuildUser user);

        /// <summary>
        /// Loads and authentication and authorization data into the service, for self-initiated operations.
        /// Self-authentication is basically a way to have self-initiated bot actions reuse code that is exposed to consumers,
        /// by having the bot user itself be registered as <see cref="CurrentUserId"/>, with no <see cref="CurrentGuildId"/>,
        /// and with a <see cref="CurrentClaims"/> collection that contains every claim that exists for the application.
        /// </summary>
        /// <param name="self">The current Discord self-user.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task OnAuthenticatedAsync(ISelfUser self);

        /// <summary>
        /// Requires that there be an authenticated guild for the current request.
        /// </summary>
        void RequireAuthenticatedGuild();

        /// <summary>
        /// Requires that there be an authenticated user for the current request.
        /// </summary>
        void RequireAuthenticatedUser();

        /// <summary>
        /// Requires that the given set of claims be present, for the current request.
        /// </summary>
        /// <param name="claims">A set of claims to be checked against <see cref="CurrentClaims"/>.</param>
        void RequireClaims(params AuthorizationClaim[] claims);
    }

    /// <inheritdoc />
    public class AuthorizationService : IAuthorizationService
    {
        public AuthorizationService(IServiceProvider serviceProvider, IDiscordClient discordClient, IClaimMappingRepository claimMappingRepository)
        {
            DiscordClient = discordClient;
            // Workaround for circular dependency.
            _lazyUserService = new Lazy<IUserService>(() => serviceProvider.GetRequiredService<IUserService>());
            ClaimMappingRepository = claimMappingRepository;
        }

        /// <inheritdoc />
        public ulong? CurrentUserId { get; internal protected set; }

        /// <inheritdoc />
        public ulong? CurrentGuildId { get; internal protected set; }

        /// <inheritdoc />
        public IReadOnlyCollection<AuthorizationClaim> CurrentClaims { get; internal protected set; }

        /// <inheritdoc />
        public async Task AutoConfigureGuildAsync(IGuild guild)
        {
            if (await ClaimMappingRepository.AnyAsync(new ClaimMappingSearchCriteria()
            {
                GuildId = guild.Id,
                IsDeleted = false,
            }))
            {
                return;
            }

            // Need the bot user to exist, before we start adding claims, created by the bot user.
            await UserService.TrackUserAsync(
                await guild.GetUserAsync(DiscordClient.CurrentUser.Id));

            using (var transaction = await ClaimMappingRepository.BeginCreateTransactionAsync())
            {
                foreach (var claim in Enum.GetValues(typeof(AuthorizationClaim)).Cast<AuthorizationClaim>())
                    foreach (var role in guild.Roles.Where(x => x.Permissions.Administrator))
                        await ClaimMappingRepository.CreateAsync(new ClaimMappingCreationData()
                        {
                            Type = ClaimMappingType.Granted,
                            GuildId = guild.Id,
                            RoleId = role.Id,
                            UserId = null,
                            Claim = claim,
                            CreatedById = DiscordClient.CurrentUser.Id
                        });

                transaction.Commit();
            }
        }

        /// <inheritdoc />
        public async Task UnConfigureGuildAsync(IGuild guild)
        {
            foreach (var claimMappingId in await ClaimMappingRepository.SearchIdsAsync(new ClaimMappingSearchCriteria()
            {
                GuildId = guild.Id,
                IsDeleted = false
            }))
            {
                await ClaimMappingRepository.TryDeleteAsync(claimMappingId, DiscordClient.CurrentUser.Id);
            }
        }

        /// <inheritdoc />
        public async Task AddClaimMapping(IRole role, ClaimMappingType type, AuthorizationClaim claim)
        {
            RequireAuthenticatedUser();
            RequireClaims(AuthorizationClaim.AuthorizationConfigure);

            using (var transaction = await ClaimMappingRepository.BeginCreateTransactionAsync())
            {
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
        }

        /// <inheritdoc />
        public async Task AddClaimMapping(IGuildUser user, ClaimMappingType type, AuthorizationClaim claim)
        {
            RequireAuthenticatedUser();
            RequireClaims(AuthorizationClaim.AuthorizationConfigure);

            using (var transaction = await ClaimMappingRepository.BeginCreateTransactionAsync())
            {
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
        }

        /// <inheritdoc />
        public async Task RemoveClaimMapping(IRole role, ClaimMappingType type, AuthorizationClaim claim)
        {
            RequireAuthenticatedUser();
            RequireClaims(AuthorizationClaim.AuthorizationConfigure);

            var mappingIds = (await ClaimMappingRepository.SearchIdsAsync(new ClaimMappingSearchCriteria()
            {
                Types = new[] { type },
                GuildId = role.Guild.Id,
                RoleIds = new[] { role.Id },
                Claims = new[] { claim },
                IsDeleted = false,
            }));

            if (!mappingIds.Any())
                throw new InvalidOperationException($"A claim mapping of type {type} to claim {claim} for role {role.Name} does not exist");

            await ClaimMappingRepository.TryDeleteAsync(mappingIds.First(), CurrentUserId.Value);
        }

        /// <inheritdoc />
        public async Task RemoveClaimMapping(IGuildUser user, ClaimMappingType type, AuthorizationClaim claim)
        {
            RequireAuthenticatedUser();
            RequireClaims(AuthorizationClaim.AuthorizationConfigure);

            var mappingIds = (await ClaimMappingRepository.SearchIdsAsync(new ClaimMappingSearchCriteria()
            {
                Types = new[] { type },
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Claims = new[] { claim },
                IsDeleted = false,
            }));

            if (!mappingIds.Any())
                throw new InvalidOperationException($"A claim mapping of type {type} to claim {claim} for user {user.GetDisplayName()} does not exist");

            await ClaimMappingRepository.TryDeleteAsync(mappingIds.First(), CurrentUserId.Value);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserClaimsAsync(IGuildUser guildUser, params AuthorizationClaim[] filterClaims)
        {
            if (guildUser == null)
                throw new ArgumentNullException(nameof(guildUser));

            if (guildUser.Id == DiscordClient.CurrentUser.Id || guildUser.GuildPermissions.Administrator)
                return Enum.GetValues(typeof(AuthorizationClaim)).Cast<AuthorizationClaim>().ToArray();

            if (guildUser.Id == CurrentUserId)
                return CurrentClaims;

            return await LookupPosessedClaimsAsync(guildUser.GuildId, guildUser.RoleIds, guildUser.Id, filterClaims);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserMissingClaimsAsync(IGuildUser guildUser, params AuthorizationClaim[] claims)
            => claims.Except(await GetGuildUserClaimsAsync(guildUser, claims))
                .ToArray();

        /// <inheritdoc />
        public async Task<bool> HasClaimsAsync(IGuildUser guildUser, params AuthorizationClaim[] claims)
            => !(await GetGuildUserMissingClaimsAsync(guildUser, claims)).Any();
        
        /// <inheritdoc />
        public async Task OnAuthenticatedAsync(IGuildUser user)
        {
            CurrentClaims = await GetGuildUserClaimsAsync(user);
            CurrentGuildId = user.GuildId;
            CurrentUserId = user.Id;
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

        /// <inheritdoc />
        public void RequireAuthenticatedGuild()
        {
            if (CurrentGuildId == null)
                // TODO: Booooo for exception-based flow control
                throw new InvalidOperationException("The current operation requires an authenticated guild.");
        }

        /// <inheritdoc />
        public void RequireAuthenticatedUser()
        {
            if (CurrentUserId == null)
                // TODO: Booooo for exception-based flow control
                throw new InvalidOperationException("The current operation requires an authenticated user.");
        }

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

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IUserService"/> for interacting with discord users within the application.
        /// </summary>
        internal protected IUserService UserService
            => _lazyUserService.Value;
        // Workaround for circular dependency.
        private readonly Lazy<IUserService> _lazyUserService;

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