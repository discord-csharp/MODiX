#nullable enable

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Discord;

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
            ISelfUserProvider selfUserProvider,
            IUserService userService)
        {
            _selfUserProvider = selfUserProvider;
            _userService = userService;
        }

        public async Task HandleNotificationAsync(
            GuildAvailableNotification notification,
            CancellationToken cancellationToken)
        {
            var selfUser = await _selfUserProvider.GetSelfUserAsync(cancellationToken);

            await _userService.TrackUserAsync(
                notification.Guild.GetUser(selfUser.Id),
                cancellationToken);
        }

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

        private readonly ISelfUserProvider _selfUserProvider;
        private readonly IUserService _userService;
    }
}
