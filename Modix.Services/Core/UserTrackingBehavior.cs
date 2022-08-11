#nullable enable

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;

using Modix.Common.Messaging;

namespace Modix.Services.Core
{
    [ServiceBinding(ServiceLifetime.Scoped)]
    public class UserTrackingBehavior
        : INotificationHandler<GuildAvailableNotification>,
            INotificationHandler<GuildMemberUpdatedNotification>,
            INotificationHandler<MessageReceivedNotification>,
            INotificationHandler<UserJoinedNotification>
    {
        public UserTrackingBehavior(
            DiscordSocketClient discordSocketClient,
            IUserService userService)
        {
            _discordSocketClient = discordSocketClient;
            _userService = userService;
        }

        public Task HandleNotificationAsync(
                GuildAvailableNotification notification,
                CancellationToken cancellationToken)
            => _userService.TrackUserAsync(
                notification.Guild.GetUser(_discordSocketClient.CurrentUser.Id),
                cancellationToken);

        public Task HandleNotificationAsync(
                GuildMemberUpdatedNotification notification,
                CancellationToken cancellationToken)
            => _userService.TrackUserAsync(
                notification.NewMember,
                cancellationToken);

        public Task HandleNotificationAsync(
                MessageReceivedNotification notification,
                CancellationToken cancellationToken)
            // Yes, there are cases where `IGuildUser.Guild` can be `null`. Webhooks, IIRC
            => ((notification.Message.Author is IGuildUser author) && (author.Guild is { }))
                ? _userService.TrackUserAsync(
                    author,
                    cancellationToken)
                : Task.CompletedTask;

        public Task HandleNotificationAsync(
                UserJoinedNotification notification,
                CancellationToken cancellationToken)
            => _userService.TrackUserAsync(
                notification.GuildUser,
                cancellationToken);

        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IUserService _userService;
    }
}
