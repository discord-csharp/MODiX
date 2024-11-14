using System.Threading;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Modix.Bot.Notifications;
using Modix.Services.Moderation;

namespace Modix.Bot.Responders;

public class GuildOnboardingResponder(GuildOnboardingService onboardingService) :
    INotificationHandler<GuildAvailableNotificationV3>,
    INotificationHandler<ChannelCreatedNotificationV3>,
    INotificationHandler<ChannelUpdatedNotificationV3>,
    INotificationHandler<JoinedGuildNotificationV3>
{
    public async Task Handle(GuildAvailableNotificationV3 notification, CancellationToken cancellationToken)
    {
        await onboardingService.AutoConfigureGuild(notification.Guild);
    }

    public async Task Handle(ChannelCreatedNotificationV3 notification, CancellationToken cancellationToken)
    {
        await onboardingService.AutoConfigureChannel(notification.Channel);
    }

    public async Task Handle(ChannelUpdatedNotificationV3 notification, CancellationToken cancellationToken)
    {
        await onboardingService.AutoConfigureChannel(notification.NewChannel);
    }

    public async Task Handle(JoinedGuildNotificationV3 notification, CancellationToken cancellationToken)
    {
        if (notification.Guild is IGuild { Available: true })
        {
            await onboardingService.AutoConfigureGuild(notification.Guild);
        }
    }
}
