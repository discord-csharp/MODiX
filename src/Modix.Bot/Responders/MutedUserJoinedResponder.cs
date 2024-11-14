using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using MediatR;
using Modix.Bot.Notifications;
using Modix.Data.Models.Moderation;
using Modix.Services.Moderation;
using Serilog;

namespace Modix.Bot.Responders;

public class MutedUserJoinedResponder(ModerationService moderationService)
    : INotificationHandler<UserJoinedNotificationV3>
{
    public Task Handle(UserJoinedNotificationV3 notification, CancellationToken cancellationToken)
        => MuteUser(notification.GuildUser);

    private async Task MuteUser(SocketGuildUser guildUser)
    {
        if (!await moderationService.AnyActiveInfractions(guildUser.Guild.Id, guildUser.Id, InfractionType.Mute))
        {
            return;
        }

        var muteRole = guildUser.Guild.Roles.FirstOrDefault(x => x.Name == ModerationService.MUTE_ROLE_NAME);

        if (muteRole is null)
        {
            return;
        }

        await guildUser.AddRoleAsync(muteRole);

        Log.Debug("User {UserId} was muted, because they have an active mute infraction", guildUser.Id);
    }
}
