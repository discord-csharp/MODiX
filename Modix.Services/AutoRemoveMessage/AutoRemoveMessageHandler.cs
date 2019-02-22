using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Modix.Services.Messages.Discord;
using Modix.Services.Messages.Modix;

namespace Modix.Services.AutoRemoveMessage
{
    public class AutoRemoveMessageHandler :
        INotificationHandler<RemovableMessageSent>,
        INotificationHandler<ReactionAdded>,
        INotificationHandler<RemovableMessageRemoved>
    {
        public AutoRemoveMessageHandler(
            IMemoryCache cache,
            IAutoRemoveMessageService autoRemoveMessageService)
        {
            Cache = cache;
            AutoRemoveMessageService = autoRemoveMessageService;
        }

        public Task Handle(RemovableMessageSent notification, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.CompletedTask;

            Cache.Set(
                GetKey(notification.Message.Id),
                new RemovableMessage()
                {
                    Message = notification.Message,
                    User = notification.User,
                },
                _messageCacheOptions);

            return Task.CompletedTask;
        }

        public async Task Handle(ReactionAdded notification, CancellationToken cancellationToken)
        {
            var key = GetKey(notification.Message.Id);

            if (cancellationToken.IsCancellationRequested
                || notification.Reaction.Emote.Name != "❌"
                || !Cache.TryGetValue(key, out RemovableMessage cachedMessage)
                || notification.Reaction.UserId != cachedMessage.User.Id)
            {
                return;
            }

            await cachedMessage.Message.DeleteAsync();

            await AutoRemoveMessageService.UnregisterRemovableMessageAsync(cachedMessage.Message);
        }

        public Task Handle(RemovableMessageRemoved notification, CancellationToken cancellationToken)
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
