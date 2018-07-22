using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;

using Modix.Data.Models.Core;
using Modix.Data.Repositories;

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
            if (await ClaimMappingRepository.AnyAsync(guild.Id))
                return;

            await UserService.TrackUserAsync(DiscordClient.CurrentUser);

            foreach(var claim in Enum.GetValues(typeof(AuthorizationClaim)).Cast<AuthorizationClaim>())
            {
                foreach(var role in guild.Roles.Where(x => x.Permissions.Administrator))
                    await ClaimMappingRepository.TryCreateAsync(new ClaimMappingCreationData()
                    {
                        Type = ClaimMappingType.Granted,
                        GuildId = guild.Id,
                        RoleId = role.Id,
                        UserId = null,
                        Claim = claim,
                        CreatedById = DiscordClient.CurrentUser.Id
                    });

                await ClaimMappingRepository.TryCreateAsync(new ClaimMappingCreationData()
                {
                    Type = ClaimMappingType.Granted,
                    GuildId = guild.Id,
                    RoleId = null,
                    UserId = DiscordClient.CurrentUser.Id,
                    Claim = claim,
                    CreatedById = DiscordClient.CurrentUser.Id
                });
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
        public Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserClaimsAsync(IGuildUser guildUser)
        {
            if (guildUser == null)
                throw new ArgumentNullException(nameof(guildUser));

            return GetGuildUserCurrentClaimsAsync(guildUser.GuildId, guildUser.RoleIds, guildUser.Id);
        }

        /// <inheritdoc />
        public async Task OnAuthenticatedAsync(ulong guildId, IEnumerable<ulong> roleIds, ulong userId)
            => CurrentClaims = await GetGuildUserCurrentClaimsAsync(guildId, roleIds, userId);

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

        private async Task CreateClaimMappingUnauthorized(ClaimMappingType type, ulong guildId, ulong? roleId, ulong? userId, AuthorizationClaim claim, ulong createdById)
        {
            if ((roleId == null) && (userId == null))
                throw new ArgumentException($"{nameof(roleId)} and {nameof(userId)} cannot both be null");

            if ((roleId != null) && (userId != null))
                throw new ArgumentException($"{nameof(roleId)} and {nameof(userId)} cannot both be given");

            var claimMappingId = await ClaimMappingRepository.TryCreateAsync(new ClaimMappingCreationData()
            {
                Type = type,
                GuildId = guildId,
                RoleId = roleId,
                UserId = userId,
                Claim = claim,
                CreatedById = createdById
            });

            if (claimMappingId == null)
            {
                var toClause = (roleId == null)
                    ? $"user {userId}"
                    : $"role {roleId}";
                throw new InvalidOperationException($"A claim mapping on guild {guildId} to {toClause} for {claim} already exists");
            }
        }

        private async Task<int> RescindClaimMappingsUnauthorized(ClaimMappingSearchCriteria criteria, ulong rescindedById)
        {
            var claimMappingIds = await ClaimMappingRepository.SearchIdsAsync(criteria);

            foreach (var claimMappingId in claimMappingIds)
                await ClaimMappingRepository.TryDeleteAsync(claimMappingId, rescindedById);

            return claimMappingIds.Count;
        }

        private async Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserCurrentClaimsAsync(ulong guildId, IEnumerable<ulong> roleIds, ulong userId)
        {
            CurrentGuildId = guildId;
            CurrentUserId = userId;

            var claims = new HashSet<AuthorizationClaim>();

            foreach (var claimMapping in (await ClaimMappingRepository
                .SearchBriefsAsync(new ClaimMappingSearchCriteria()
                {
                    GuildId = guildId,
                    RoleIds = roleIds.ToArray(),
                    UserId = userId,
                    IsDeleted = false
                }))
                // Evaluate role mappings (userId is null) first, to give user mappings precedence.
                .OrderBy(x => x.UserId)
                // Evaluate granted mappings first, to give denied mappings precedence.
                .ThenBy(x => x.Type))
            {
                if (claimMapping.Type == ClaimMappingType.Granted)
                    claims.Add(claimMapping.Claim);
                else
                    claims.Remove(claimMapping.Claim);
            }

            return claims;
        }
    }
}
