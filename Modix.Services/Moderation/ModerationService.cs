﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;

using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;

using Modix.Services.Core;

namespace Modix.Services.Moderation
{
    /// <inheritdoc />
    public class ModerationService : IModerationService
    {
        /// <summary>
        /// The name to be used for the role in each guild that mutes users.
        /// </summary>
        // TODO: Push this to a bot-wide config? Or maybe on a per-guild basis, but with a bot-wide default, that's pulled from config?
        public const string MuteRoleName
            = "MODiX_Moderation_Mute";

        /// <summary>
        /// Creates a new <see cref="ModerationService"/>.
        /// </summary>
        /// <param name="discordClient">The value to use for <see cref="DiscordClient"/>.</param>
        /// <param name="authorizationService">The value to use for <see cref="AuthorizationService"/>.</param>
        /// <param name="userService">The value to use for <see cref="UserService"/>.</param>
        /// <param name="moderationConfigRepository">The value to use for <see cref="ModerationConfigRepository"/>.</param>
        /// <param name="moderationActionRepository">The value to use for <see cref="ModerationActionRepository"/>.</param>
        /// <param name="infractionRepository">The value to use for <see cref="InfractionRepository"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for all parameters.</exception>
        public ModerationService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IGuildService guildService,
            IUserService userService,
            IModerationConfigRepository moderationConfigRepository,
            IModerationActionRepository moderationActionRepository,
            IInfractionRepository infractionRepository)
        {
            DiscordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            AuthorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            GuildService = guildService ?? throw new ArgumentNullException(nameof(guildService));
            UserService = userService ?? throw new ArgumentNullException(nameof(userService));
            ModerationConfigRepository = moderationConfigRepository ?? throw new ArgumentNullException(nameof(moderationConfigRepository));
            ModerationActionRepository = moderationActionRepository ?? throw new ArgumentNullException(nameof(moderationActionRepository));
            InfractionRepository = infractionRepository ?? throw new ArgumentNullException(nameof(infractionRepository));
        }

        /// <inheritdoc />
        public async Task AutoConfigureGuldAsync(IGuild guild)
        {
            var muteRole = await GetOrCreateMuteRoleAsync(guild);

            foreach (var channel in await guild.GetChannelsAsync())
                await ConfigureChannelMuteRolePermissions(channel, muteRole);

            await CreateOrUpdateConfig(guild, muteRole);
        }

        /// <inheritdoc />
        public async Task AutoConfigureChannelAsync(IChannel channel)
        {
            if (channel is IGuildChannel guildChannel)
            {
                var muteRole = await GetOrCreateMuteRoleAsync(guildChannel.Guild);

                await ConfigureChannelMuteRolePermissions(guildChannel, muteRole);

                await CreateOrUpdateConfig(guildChannel.Guild, muteRole);
            }
        }

        /// <inheritdoc />
        public async Task UnConfigureGuildAsync(IGuild guild)
        {
            var config = await ModerationConfigRepository.ReadAsync(guild.Id);
            if(config != null)
            {
                IDeletable muteRole = guild.Roles.FirstOrDefault(x => x.Id == config.MuteRoleId);
                if (muteRole != null)
                    await muteRole.DeleteAsync();

                await ModerationConfigRepository.DeleteAsync(config.GuildId);
            }
        }

        /// <inheritdoc />
        public async Task CreateInfractionAsync(InfractionType type, ulong subjectId, string reason, TimeSpan? duration)
        {
            AuthorizationService.RequireClaims(_createInfractionClaimsByType[type]);

            switch (type)
            {
                case InfractionType.Mute:
                    await DoDiscordMuteAsync(subjectId);
                    break;

                case InfractionType.Ban:
                    await DoDiscordBanAsync(subjectId);
                    break;
            }
            
            var actionId = await ModerationActionRepository.CreateAsync(new ModerationActionCreationData()
            {
                Type = ModerationActionType.InfractionCreated,
                CreatedById = AuthorizationService.CurrentUserId.Value,
                Reason = reason
            });

            var infractionId = await InfractionRepository.CreateAsync(new InfractionCreationData()
            {
                Type = type,
                SubjectId = subjectId,
                Duration = duration,
                CreateActionId = actionId
            });

            await ModerationActionRepository.UpdateAsync(actionId, data =>
            {
                data.InfractionId = infractionId;
            });

            // TODO: Log action to a channel, pulled from IModerationConfigRepository. 
            
            // TODO: Implement InfractionAutoExpirationBehavior (or whatever) to automatically rescind infractions, based on Duration, and notify it here that a new infraction has been created, if it has a duration.
        }

        /// <inheritdoc />
        public async Task RescindInfractionAsync(long infractionId, string reason)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRescind);

            var infraction = await InfractionRepository.ReadAsync(infractionId);
            if (infraction == null)
                throw new ArgumentException("Infraction does not exist", nameof(infractionId));

            switch (infraction.Type)
            {
                case InfractionType.Mute:
                    await DoDiscordUnMuteAsync(infraction.Subject.Id);
                    break;

                case InfractionType.Ban:
                    await DoDiscordUnBanAsync(infraction.Subject.Id);
                    break;
            }

            var actionId = await ModerationActionRepository.CreateAsync(new ModerationActionCreationData()
            {
                Type = ModerationActionType.InfractionRescinded,
                CreatedById = AuthorizationService.CurrentUserId.Value,
                Reason = reason,
                InfractionId = infractionId
            });

            // TODO: Log action to a channel, pulled from IModerationConfigRepository. 
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return await InfractionRepository.SearchSummariesAsync(searchCriteria, sortingCriteria);
        }

        /// <inheritdoc />
        public async Task<RecordsPage<InfractionSummary>> SearchInfractionsAsync(InfractionSearchCriteria searchCriteria, IEnumerable<SortingCriteria> sortingCriteria, PagingCriteria pagingCriteria)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ModerationRead);

            return await InfractionRepository.SearchSummariesPagedAsync(searchCriteria, sortingCriteria, pagingCriteria);
        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// A <see cref="IAuthorizationService"/> to be used to interact with frontend authentication system, and perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IGuildService"/> for interacting with discord guild within the application.
        /// </summary>
        internal protected IGuildService GuildService { get; }

        /// <summary>
        /// An <see cref="IUserService"/> for interacting with discord users within the application.
        /// </summary>
        internal protected IUserService UserService { get; }

        /// <summary>
        /// An <see cref="IModerationConfigRepository"/> for storing and retrieving moderation configuration data.
        /// </summary>
        internal protected IModerationConfigRepository ModerationConfigRepository { get; }

        /// <summary>
        /// An <see cref="IModerationActionRepository"/> for storing and retrieving moderation action data.
        /// </summary>
        internal protected IModerationActionRepository ModerationActionRepository { get; }

        /// <summary>
        /// An <see cref="IInfractionRepository"/> for storing and retrieving infraction data.
        /// </summary>
        internal protected IInfractionRepository InfractionRepository { get; }

        private async Task CreateOrUpdateConfig(IGuild guild, IRole muteRole)
        {
            var config = await ModerationConfigRepository.ReadAsync(guild.Id);
            if (config == null)
            {
                await ModerationConfigRepository.CreateAsync(new ModerationConfigCreationData()
                {
                    GuildId = guild.Id,
                    MuteRoleId = muteRole.Id
                });
            }
            else if (muteRole.Id != config.MuteRoleId)
            {
                await ModerationConfigRepository.UpdateAsync(config.GuildId, data =>
                {
                    data.MuteRoleId = muteRole.Id;
                });
            }
        }

        private Task ConfigureChannelMuteRolePermissions(IGuildChannel channel, IRole muteRole)
        {
            // TODO: GetPermissionOverwrite and AddPermissionOverwriteAsync are bugged in Discord.NET 1.0.2.
            // Probably need to upgrade Discord.NET to get this functionality.
            return Task.CompletedTask;

            //var permissionOverwrite = channel.GetPermissionOverwrite(muteRole);
            //if (permissionOverwrite != null)
            //{
            //    if ((permissionOverwrite.Value.AllowValue == _mutePermissions.AllowValue) &&
            //        (permissionOverwrite.Value.DenyValue == _mutePermissions.DenyValue))
            //        return;

            //    await channel.RemovePermissionOverwriteAsync(muteRole);
            //}

            //await channel.AddPermissionOverwriteAsync(muteRole, _mutePermissions);
        }

        private async Task DoDiscordMuteAsync(ulong subjectId)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();

            var guild = await GuildService.GetGuildAsync(AuthorizationService.CurrentGuildId.Value);
            var muteRole = await GetOrCreateMuteRoleAsync(guild);
            var subject = await UserService.GetGuildUserAsync(guild.Id, subjectId);

            if (subject.RoleIds.Contains(muteRole.Id))
                throw new InvalidOperationException($"Discord user {subjectId} is already muted");

            await subject.AddRoleAsync(muteRole);
        }

        private async Task DoDiscordUnMuteAsync(ulong subjectId)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();

            var guild = await GuildService.GetGuildAsync(AuthorizationService.CurrentGuildId.Value);
            var muteRole = await GetOrCreateMuteRoleAsync(guild);
            var subject = await UserService.GetGuildUserAsync(guild.Id, subjectId);

            if (!subject.RoleIds.Contains(muteRole.Id))
                throw new InvalidOperationException($"Discord user {subjectId} is not currently muted");

            await subject.AddRoleAsync(muteRole);
        }

        private async Task DoDiscordBanAsync(ulong subjectId)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();

            var guild = await GuildService.GetGuildAsync(AuthorizationService.CurrentGuildId.Value);
            var subject = await UserService.GetGuildUserAsync(guild.Id, subjectId);

            if ((await guild.GetBansAsync()).Any(x => x.User.Id == subject.Id))
                throw new InvalidOperationException($"Discord user {subjectId} is already banned");

            await guild.AddBanAsync(subject);
        }

        private async Task DoDiscordUnBanAsync(ulong subjectId)
        {
            AuthorizationService.RequireAuthenticatedGuild();
            AuthorizationService.RequireAuthenticatedUser();

            var guild = await GuildService.GetGuildAsync(AuthorizationService.CurrentGuildId.Value);
            var subject = await UserService.GetGuildUserAsync(guild.Id, subjectId);

            if (!(await guild.GetBansAsync()).Any(x => x.User.Id == subject.Id))
                throw new InvalidOperationException($"Discord user {subjectId} is not currently banned");

            await guild.AddBanAsync(subject);
        }

        private async Task<IRole> GetOrCreateMuteRoleAsync(IGuild guild)
            => guild.Roles.FirstOrDefault(x => x.Name == MuteRoleName)
                ?? await guild.CreateRoleAsync(MuteRoleName);

        // Unused, because ConfigureChannelMuteRolePermissions is currently disabled.
        private static readonly OverwritePermissions _mutePermissions
            = new OverwritePermissions(
                sendMessages: PermValue.Deny,
                speak: PermValue.Deny);

        private static readonly Dictionary<InfractionType, AuthorizationClaim> _createInfractionClaimsByType
            = new Dictionary<InfractionType, AuthorizationClaim>()
            {
                {InfractionType.Notice, AuthorizationClaim.ModerationNote },
                {InfractionType.Warning, AuthorizationClaim.ModerationWarn },
                {InfractionType.Mute, AuthorizationClaim.ModerationMute },
                {InfractionType.Ban, AuthorizationClaim.ModerationBan }
            };
    }
}
