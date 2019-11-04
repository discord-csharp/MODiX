using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Modix.Common.Messaging;
using Modix.Services.Core;
using Modix.Services.Moderation;
using Serilog;

namespace Modix.Services.DuplicitMessage
{
    public class DuplicitMessageHandler :
        INotificationHandler<MessageReceivedNotification>,
        INotificationHandler<MessageDeletedNotification>,
        INotificationHandler<UserLeftNotification>
    {
        private readonly Dictionary<ulong, MessageInfo> _messages = new Dictionary<ulong, MessageInfo>();

        /// <summary>
        /// An <see cref="ISelfUserProvider"/> used to interact with the current bot user.
        /// </summary>
        internal protected ISelfUserProvider SelfUserProvider { get; }

        /// <summary>
        /// An <see cref="IServiceScopeFactory"/> used to generate service scopes for dispatched messages to be processed within.
        /// </summary>
        internal protected IServiceScopeFactory ServiceScopeFactory { get; }

        /// <summary>
        /// Maximum time between for 2 identical messages to recognize, in seconds
        /// </summary>
        public double MaximumSecondsBetweenMessages { get; set; } = 60;

        /// <summary>
        /// Minimum message length to check
        /// </summary>
        public double MinimumMessageLength { get; set; } = 5;

        public DuplicitMessageHandler(ISelfUserProvider selfUserProvider, IServiceScopeFactory serviceScopeFactory)
        {
            SelfUserProvider = selfUserProvider;
            ServiceScopeFactory = serviceScopeFactory;
        }

        /// <inheritdoc />
        public Task HandleNotificationAsync(MessageReceivedNotification notification, CancellationToken cancellationToken = default)
            => CheckForDuplicitMessage(notification.Message);

        /// <inheritdoc />
        public Task HandleNotificationAsync(MessageDeletedNotification notification, CancellationToken cancellationToken = default)
            => TryRemoveLastMessage(notification.Message.Value);

        /// <inheritdoc />
        public Task HandleNotificationAsync(UserLeftNotification notification, CancellationToken cancellationToken = default)
            => RemoveLastMessageByUser(notification.GuildUser);
        
        private async Task CheckForDuplicitMessage(IMessage message)
        {
            if
            (
                !(message.Author is IGuildUser author) || (author.Guild is null) ||
                !(message.Channel is IGuildChannel channel) || (channel.Guild is null)
            )
            {
                Log.Debug("Message {MessageId} was not in an IGuildChannel & IMessageChannel, or Author {Author} was not an IGuildUser",
                    message.Id, message.Author.Id);
                return;
            }

            if (message.Content.Length < MinimumMessageLength)
                return;

            var selfUser = await SelfUserProvider.GetSelfUserAsync();
            if (author.Id == selfUser.Id)
                return;

            var found = _messages.TryGetValue(author.Id, out var lastMessage);
            _messages[author.Id] = new MessageInfo(message);

            if (found && message.Content == lastMessage.Content)
            {
                var deltaSeconds = (message.Timestamp - lastMessage.Timestamp).TotalSeconds;
                if (deltaSeconds < MaximumSecondsBetweenMessages)
                {
                    Log.Debug("Message {MessageId} is going to be deleted", message.Id);
                    using var serviceScope = ServiceScopeFactory.CreateScope();
                    var moderationService = serviceScope.ServiceProvider.GetRequiredService<IModerationService>();
                    await moderationService.DeleteMessageAsync(message, "Duplicit message detected", selfUser.Id);

                    Log.Debug("Message {MessageId} was deleted because it was detected as duplicate", message.Id);
                    await message.Channel.SendMessageAsync($"Sorry {author.Mention} your message has been removed - please don't post duplicit messages");
                }
            }
        }

        private Task TryRemoveLastMessage(IMessage message)
        {
            if (message.Author is IGuildUser && _messages.TryGetValue(message.Author.Id, out var lastMessage) && lastMessage.Timestamp == message.Timestamp)
            {
                _messages.Remove(message.Author.Id);
            }
            return Task.CompletedTask;
        }

        private Task RemoveLastMessageByUser(IUser user)
        {
            _messages.Remove(user.Id);
            return Task.CompletedTask;
        }

        private struct MessageInfo
        {
            public string Content { get; }
            public DateTimeOffset Timestamp { get; }

            public MessageInfo(IMessage message)
            {
                Content = message.Content;
                Timestamp = message.Timestamp;
            }
        }
    }
}
