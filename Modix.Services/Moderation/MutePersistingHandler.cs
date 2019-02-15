using System.Threading;
using System.Threading.Tasks;

using Discord;

using MediatR;

using Modix.Data.Models.Moderation;
using Modix.Services.Messages.Discord;

using Serilog;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Implements a handler that persists mutes for users who leave and rejoin a guild.
    /// </summary>
    public class MutePersistingHandler :
        INotificationHandler<UserJoined>
    {
        private readonly IModerationService _moderationService;
        private readonly ISelfUser _botUser;

        /// <summary>
        /// Constructs a new <see cref="MutePersistingHandler"/> object with the given injected dependencies.
        /// </summary>
        /// <param name="moderationService">A moderation service to interact with the infractions system.</param>
        /// <param name="botUser">The Discord user that the bot is running as.</param>
        public MutePersistingHandler(
            IModerationService moderationService,
            ISelfUser botUser)
        {
            _moderationService = moderationService;
            _botUser = botUser;
        }

        public Task Handle(UserJoined notification, CancellationToken cancellationToken)
            => TryMuteUserAsync(notification.Guild, notification.User);

        /// <summary>
        /// Mutes the user if they have an active mute infraction in the guild.
        /// </summary>
        /// <param name="guild">The guild that the user joined.</param>
        /// <param name="user">The user who joined the guild.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes.
        /// </returns>
        private async Task TryMuteUserAsync(IGuild guild, IGuildUser user)
        {
            var userHasActiveMuteInfraction = await _moderationService.AnyInfractionsAsync(new InfractionSearchCriteria()
            {
                GuildId = guild.Id,
                IsDeleted = false,
                IsRescinded = false,
                SubjectId = user.Id,
                Types = new[] { InfractionType.Mute },
            });

            if (!userHasActiveMuteInfraction)
            {
                Log.Debug("User {0} was not muted, because they do not have any active mute infractions.", user.Id);
                return;
            }

            var muteRole = await _moderationService.GetOrCreateDesignatedMuteRoleAsync(guild, _botUser.Id);

            Log.Debug("User {0} was muted, because they have an active mute infraction.", user.Id);

            await user.AddRoleAsync(muteRole);
        }
    }
}
