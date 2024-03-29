using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;

using Microsoft.Extensions.Caching.Memory;

using Modix.Common.Messaging;

namespace Modix.Services.AutoRemoveMessage
{
    public class AutoRemoveMessageHandler :
        INotificationHandler<ReactionAddedNotification>,
        INotificationHandler<RemovableMessageRemovedNotification>,
        INotificationHandler<RemovableMessageSentNotification>
    {
        public AutoRemoveMessageHandler(
            IMemoryCache cache,
            IAutoRemoveMessageService autoRemoveMessageService)
        {
            Cache = cache;
            AutoRemoveMessageService = autoRemoveMessageService;
        }

        public Task HandleNotificationAsync(RemovableMessageSentNotification notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.CompletedTask;

            Cache.Set(
                GetKey(notification.Message.Id),
                new RemovableMessage()
                {
                    Message = notification.Message,
                    Users = notification.Users,
                },
                _messageCacheOptions);

            return Task.CompletedTask;
        }

        public async Task HandleNotificationAsync(ReactionAddedNotification notification, CancellationToken cancellationToken)
        {
            var key = GetKey(notification.Message.Id);

            if (cancellationToken.IsCancellationRequested
                || notification.Reaction.Emote.Name != "❌"
                || !Cache.TryGetValue(key, out RemovableMessage cachedMessage)
                || !cachedMessage.Users.Any(user => user.Id == notification.Reaction.UserId))
            {
                return;
            }

            await cachedMessage.Message.DeleteAsync();

            AutoRemoveMessageService.UnregisterRemovableMessage(cachedMessage.Message);
        }

        public Task HandleNotificationAsync(RemovableMessageRemovedNotification notification, CancellationToken cancellationToken)
        {
            var key = GetKey(notification.Message.Id);

            if (cancellationToken.IsCancellationRequested
                || !Cache.TryGetValue(key, out _))
            {
                return Task.CompletedTask;
            }

            Cache.Remove(key);

            return Task.CompletedTask;
        }

        protected IMemoryCache Cache { get; }

        protected IAutoRemoveMessageService AutoRemoveMessageService { get; }

        private static object GetKey(ulong messageId)
            => new
            {
                MessageId = messageId,
                Target = "RemovableMessage",
            };

        private static readonly MemoryCacheEntryOptions _messageCacheOptions =
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
    }
}
