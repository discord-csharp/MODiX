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

        /// <summary>
        /// Automatically configures mention mappings for the supplied role.
        /// </summary>
        /// <param name="role">The role to be configured.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AutoConfigureRoleAsync(IRole role);

        /// <summary>
        /// Determines whether the user can mention the supplied role.
        /// </summary>
        /// <param name="role">The role that the user is trying to mention.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="role"/>.</exception>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes,
        /// with a flag indicating whether the user can mention <paramref name="role"/>.
        /// </returns>
        Task<bool> CanUserMentionAsync(IRole role);

        /// <summary>
        /// Ensures that the role can be mentioned.
        /// </summary>
        /// <param name="role">The role to be mentioned.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes,
        /// with a delegate that can be invoked to restore the role to its previous configuration.
        /// </returns>
        Task<Func<Task>> EnsureMentionable(IRole role);

        /// <summary>
        /// Retireves mention mapping data for the supplied role ID.
        /// </summary>
        /// <param name="roleId">The Discord snowflake ID of the role for which the mapping is to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// with a <see cref="MentionMappingSummary"/> describing the retrieved mapping.
        /// </returns>
        Task<MentionMappingSummary> GetMentionMappingAsync(ulong roleId);

        /// <summary>
        /// Attempts to modify the mention mapping for the supplied role ID.
        /// </summary>
        /// <param name="roleId">The <see cref="MentionMappingEntity.RoleId"/> value of the mapping to be modified.</param>
        /// <param name="updateAction">An action that describes how to modify the mapping.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="updateAction"/>.</exception>
        /// <returns>
        /// A <see cref="task"/> that will complete when the operation completes,
        /// with a flag indicating whether the operation was successful.
        /// </returns>
        Task<bool> TryUpdateMentionMappingAsync(ulong roleId, Action<MentionMappingMutationData> updateAction);
    }

    /// <inheritdoc />
    internal class MentionService : IMentionService
    {
        public MentionService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IUserService userService,
            IMentionMappingRepository mentionMappingRepository,
            IDesignatedRoleMappingRepository designatedRoleMappingRepository)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            UserService = userService;
            MentionMappingRepository = mentionMappingRepository;
            DesignatedRoleMappingRepository = designatedRoleMappingRepository;
        }

        /// <inheritdoc />
        public async Task AutoConfigureGuildAsync(IGuild guild)
        {
            if (guild is null)
                return;

            var mappings = await MentionMappingRepository.ReadByGuildAsync(guild.Id);

            foreach (var deletionCandidate in mappings.Where(x => !guild.Roles.Select(r => r.Id).Contains(x.RoleId)))
            {
                await MentionMappingRepository.TryDeleteAsync(deletionCandidate.RoleId);
            }

            foreach (var role in guild.Roles)
            {
                await EnsureValidMappingAsync(role);
            }
        }

        /// <inheritdoc />
        public async Task AutoConfigureRoleAsync(IRole role)
        {
            if (role is null)
                return;

            await EnsureValidMappingAsync(role);
        }

        /// <inheritdoc />
        public async Task<bool> CanUserMentionAsync(IRole role)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();

            if (role.IsMentionable)
                return true;

            var user = await UserService.GetGuildUserAsync(AuthorizationService.CurrentGuildId.Value, AuthorizationService.CurrentUserId.Value);

            if (user.GuildPermissions.Administrator || user.GuildPermissions.MentionEveryone)
                return true;

            var mentionMapping = await MentionMappingRepository.ReadAsync(role.Id);

            if (mentionMapping.Mentionability == MentionabilityType.NotMentionable)
                return false;

            if (mentionMapping.MinimumRank is null)
                return true;

            var userRank = (await DesignatedRoleMappingRepository.SearchBriefsAsync(new DesignatedRoleMappingSearchCriteria
                {
                    GuildId = AuthorizationService.CurrentGuildId.Value,
                    Type = DesignatedRoleType.Rank,
                    IsDeleted = false,
                }))
                .Select(x => x.Role)
                .OrderByDescending(x => x.Position)
                .FirstOrDefault(x => user.RoleIds.Contains(x.Id));

            return userRank?.Position >= mentionMapping.MinimumRank.Position;
        }

        public async Task<Func<Task>> EnsureMentionable(IRole role)
        {
            if (!role.IsMentionable)
            {
                await role.ModifyAsync(x => x.Mentionable = true);

                return async () => await role.ModifyAsync(x => x.Mentionable = false);
            }

            return () => Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<MentionMappingSummary> GetMentionMappingAsync(ulong roleId)
            => await MentionMappingRepository.ReadAsync(roleId);

        /// <inheritdoc />
        public async Task<bool> TryUpdateMentionMappingAsync(ulong roleId, Action<MentionMappingMutationData> updateAction)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();
            AuthorizationService.RequireClaims(AuthorizationClaim.MentionConfigure);

            if (updateAction is null)
                throw new ArgumentNullException(nameof(updateAction));

            bool succeeded;

            using (var transaction = await MentionMappingRepository.BeginUpdateTransactionAsync())
            {
                succeeded = await MentionMappingRepository.TryUpdateAsync(roleId, updateAction);

                transaction.Commit();
            }

            var mentionMapping = await MentionMappingRepository.ReadAsync(roleId);
            var guild = await DiscordClient.GetGuildAsync(AuthorizationService.CurrentGuildId.Value);
            var role = guild.GetRole(roleId);

            switch (mentionMapping.Mentionability)
            {
                case MentionabilityType.NotMentionable:
                    await role.ModifyAsync(x => x.Mentionable = false);
                    break;
                case MentionabilityType.ModixCommand:
                    await role.ModifyAsync(x => x.Mentionable = false);
                    break;
                case MentionabilityType.DiscordSyntaxAndModixCommand:
                    await role.ModifyAsync(x => x.Mentionable = true);
                    break;
            }

            return succeeded;
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
        /// An <see cref="IUserService"/> for interacting with discord users within the application.
        /// </summary>
        internal protected IUserService UserService { get; }

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

            if (await EnsureMappingIsCreatedAsync(mentionMapping, role))
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

        private async Task<bool> EnsureMappingIsCreatedAsync(MentionMappingSummary mentionMapping, IRole role)
        {
            if (mentionMapping is null)
            {
                await CreateMappingAsync(role);
                return true;
            }

            return false;
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
