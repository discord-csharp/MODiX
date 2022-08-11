using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;
using Modix.Data.Models.Moderation;
using Modix.Services.Core;

using Serilog;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Implements a handler that persists mutes for users who leave and rejoin a guild.
    /// </summary>
    public class MutePersistingHandler :
        INotificationHandler<UserJoinedNotification>
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IModerationService _moderationService;

        /// <summary>
        /// Constructs a new <see cref="MutePersistingHandler"/> object with the given injected dependencies.
        /// </summary>
        /// <param name="moderationService">A moderation service to interact with the infractions system.</param>
        /// <param name="selfUserProvider">The Discord user that the bot is running as.</param>
        public MutePersistingHandler(
            DiscordSocketClient discordSocketClient,
            IModerationService moderationService)
        {
            _discordSocketClient = discordSocketClient;
            _moderationService = moderationService;
        }

        public Task HandleNotificationAsync(UserJoinedNotification notification, CancellationToken cancellationToken)
            => TryMuteUserAsync(notification.GuildUser);

        /// <summary>
        /// Mutes the user if they have an active mute infraction in the guild.
        /// </summary>
        /// <param name="guild">The guild that the user joined.</param>
        /// <param name="user">The user who joined the guild.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        private async Task TryMuteUserAsync(SocketGuildUser guildUser)
        {
            var userHasActiveMuteInfraction = await _moderationService.AnyInfractionsAsync(new InfractionSearchCriteria()
            {
                GuildId = guildUser.Guild.Id,
                IsDeleted = false,
                IsRescinded = false,
                SubjectId = guildUser.Id,
                Types = new[] { InfractionType.Mute },
            });

            if (!userHasActiveMuteInfraction)
            {
                Log.Debug("User {0} was not muted, because they do not have any active mute infractions.", guildUser.Id);
                return;
            }

            var muteRole = await _moderationService.GetOrCreateDesignatedMuteRoleAsync(guildUser.Guild, _discordSocketClient.CurrentUser.Id);

            Log.Debug("User {0} was muted, because they have an active mute infraction.", guildUser.Id);

            await guildUser.AddRoleAsync(muteRole);
        }
    }
}
