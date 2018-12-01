using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;

using Modix.Data.Models.Core;
using Modix.Data.Models.Mentions;
using Modix.Data.Repositories;
using Modix.Services.Core;

namespace Modix.Services.Mentions
{
    public interface IMentionService
    {
        /// <summary>
        /// Automatically configures mention mappings for the supplied guild.
        /// </summary>
        /// <param name="guild">The guild to be configured.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AutoConfigureGuildAsync(IGuild guild);
    }

    /// <inheritdoc />
    internal class MentionService : IMentionService
    {
        public MentionService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IMentionMappingRepository mentionMappingRepository,
            IDesignatedRoleMappingRepository designatedRoleMappingRepository)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            MentionMappingRepository = mentionMappingRepository;
            DesignatedRoleMappingRepository = designatedRoleMappingRepository;
        }

        /// <inheritdoc />
        public async Task AutoConfigureGuildAsync(IGuild guild)
        {
            if (guild is null)
                return;

            foreach (var role in guild.Roles)
            {
                await EnsureValidMappingAsync(role);
            }
        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IAuthorizationService"/> to be used to interact with frontend authentication system, and perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IMentionMappingRepository"/> for storing and retrieving mention mapping data.
        /// </summary>
        internal protected IMentionMappingRepository MentionMappingRepository { get; }

        /// <summary>
        /// An <see cref="IDesignatedRoleMappingRepository"/> for storing and retrieving role designation data.
        /// </summary>
        internal protected IDesignatedRoleMappingRepository DesignatedRoleMappingRepository { get; }

        private async Task EnsureValidMappingAsync(IRole role)
        {
            var mentionMapping = await MentionMappingRepository.ReadAsync(role.Id);

            await EnsureMappingIsCreatedAsync(mentionMapping, role);

            mentionMapping = await MentionMappingRepository.ReadAsync(role.Id);

            if (role.IsMentionable)
            {
                await EnsureMappingForIsMentionableAsync(mentionMapping, role);
            }
            else
            {
                await EnsureMappingForNotIsMentionableAsync(mentionMapping, role);
            }
        }

        private async Task EnsureMappingIsCreatedAsync(MentionMappingSummary mentionMapping, IRole role)
        {
            if (mentionMapping is null)
                await CreateMappingAsync(role);
        }

        private async Task EnsureMappingForIsMentionableAsync(MentionMappingSummary mentionMapping, IRole role)
        {
            if (mentionMapping.Mentionability != MentionabilityType.DiscordSyntaxAndModixCommand
                || !(mentionMapping.MinimumRank is null))
            {
                using (var transaction = await MentionMappingRepository.BeginUpdateTransactionAsync())
                {
                    if (!await MentionMappingRepository.TryUpdateAsync(mentionMapping.RoleId,
                        x =>
                        {
                            x.Mentionability = MentionabilityType.DiscordSyntaxAndModixCommand;
                            x.MinimumRankId = null;
                        }))
                        throw new InvalidOperationException($"Unable to find a mention mapping for the {mentionMapping.Role.Name} role.");

                    transaction.Commit();
                }
            }
        }

        private async Task EnsureMappingForNotIsMentionableAsync(MentionMappingSummary mentionMapping, IRole role)
        {
            switch (mentionMapping.Mentionability)
            {
                case MentionabilityType.NotMentionable:
                    await EnsureNoMinimumRankAsync(mentionMapping);
                    break;
                case MentionabilityType.ModixCommand:
                    await EnsureMinimumRankIsValidAsync(mentionMapping);
                    break;
                case MentionabilityType.DiscordSyntaxAndModixCommand:
                    await UpdateMappingForNotIsMentionableDiscordSyntaxAndModixCommandAsync(mentionMapping);
                    await EnsureMinimumRankIsValidAsync(mentionMapping);
                    break;
            }
        }

        private async Task EnsureMinimumRankIsValidAsync(MentionMappingSummary mentionMapping)
        {
            if (mentionMapping.MinimumRank is null)
                return;

            if (await IsValidRankRoleAsync(mentionMapping.MinimumRank))
                return;

            var newMinimum = await GetNextAvailableRankRoleAsync(mentionMapping.MinimumRank);

            if (newMinimum is null)
                return;

            using (var transaction = await MentionMappingRepository.BeginUpdateTransactionAsync())
            {
                if (!await MentionMappingRepository.TryUpdateAsync(mentionMapping.RoleId, x => x.MinimumRankId = newMinimum.Id))
                    throw new InvalidOperationException($"Unable to find a mention mapping for the {mentionMapping.Role.Name} role.");

                transaction.Commit();
            }
        }

        private async Task CreateMappingAsync(IRole role)
        {
            using (var transaction = await MentionMappingRepository.BeginCreateTransactionAsync())
            {
                await MentionMappingRepository.CreateAsync(new MentionMappingCreationData
                {
                    GuildId = role.Guild.Id,
                    RoleId = role.Id,
                    Mentionability = role.IsMentionable
                        ? MentionabilityType.DiscordSyntaxAndModixCommand
                        : MentionabilityType.NotMentionable,
                    MinimumRankId = null,
                });

                transaction.Commit();
            }
        }

        private async Task UpdateMappingForNotIsMentionableDiscordSyntaxAndModixCommandAsync(MentionMappingSummary mentionMapping)
        {
            using (var transaction = await MentionMappingRepository.BeginCreateTransactionAsync())
            {
                if (!await MentionMappingRepository.TryUpdateAsync(mentionMapping.RoleId,
                    x =>
                    {
                        x.Mentionability = mentionMapping.MinimumRank is null
                            ? MentionabilityType.NotMentionable
                            : MentionabilityType.ModixCommand;
                        x.MinimumRankId = mentionMapping.MinimumRank?.Id;
                    }))
                    throw new InvalidOperationException($"Unable to find a mention mapping for the {mentionMapping.Role.Name} role.");

                transaction.Commit();
            }
        }

        private async Task EnsureNoMinimumRankAsync(MentionMappingSummary mentionMapping)
        {
            if (mentionMapping.MinimumRank is null)
                return;

            using (var transaction = await MentionMappingRepository.BeginUpdateTransactionAsync())
            {
                if (!await MentionMappingRepository.TryUpdateAsync(mentionMapping.RoleId, x => x.MinimumRankId = null))
                    throw new InvalidOperationException($"Unable to find a mention mapping for the {mentionMapping.Role.Name} role.");

                transaction.Commit();
            }
        }

        private async Task<bool> IsValidRankRoleAsync(GuildRoleBrief role)
            => await DesignatedRoleMappingRepository.AnyAsync(new DesignatedRoleMappingSearchCriteria
            {
                RoleId = role.Id,
                Type = DesignatedRoleType.Rank,
                IsDeleted = false,
            });

        private async Task<GuildRoleBrief> GetNextAvailableRankRoleAsync(GuildRoleBrief role)
        {
            var rankRoles = (await DesignatedRoleMappingRepository.SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria
            {
                Type = DesignatedRoleType.Rank,
                IsDeleted = false,
            }))
            .Select(x => x.Role)
            .OrderBy(x => x.Position);

            if (!rankRoles.Any())
                return null;

            var nextRank = rankRoles.FirstOrDefault(x => x.Position >= role.Position);

            return nextRank
                ?? rankRoles.Last();
        }
    }
}
