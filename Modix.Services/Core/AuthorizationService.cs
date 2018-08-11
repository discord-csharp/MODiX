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
    /// <inheritdoc />
    public class AuthorizationService : IAuthorizationService
    {
        public AuthorizationService(IServiceProvider serviceProvider, IDiscordClient discordClient, IClaimMappingRepository claimMappingRepository)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            // Workaround for circular dependency.
            _lazyUserService = new Lazy<IUserService>(() => serviceProvider.GetRequiredService<IUserService>());
            ClaimMappingRepository = claimMappingRepository ?? throw new ArgumentNullException(nameof(claimMappingRepository));
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
                    Types = new [] { type },
                    GuildId = role.Guild.Id,
                    RoleIds = new [] { role.Id },
                    Claims = new [] { claim },
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

            if(!mappingIds.Any())
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

            if (guildUser.Id == DiscordClient.CurrentUser.Id)
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
        public async Task OnAuthenticatedAsync(ulong guildId, IEnumerable<ulong> roleIds, ulong userId)
        {
            CurrentClaims = await LookupPosessedClaimsAsync(guildId, roleIds, userId);
            CurrentGuildId = guildId;
            CurrentUserId = userId;
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
                throw new InvalidOperationException("The current operation requires an authenticated guild.");
        }

        /// <inheritdoc />
        public void RequireClaims(params AuthorizationClaim[] claims)
        {
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
