using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
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
        private readonly Dictionary<ulong, LastMessage> _messages = new Dictionary<ulong, LastMessage>();

        /// <summary>
        /// An <see cref="ISelfUserProvider"/> used to interact with the current bot user.
        /// </summary>
        internal protected ISelfUserProvider SelfUserProvider { get; }

        /// <summary>
        /// An <see cref="IModerationService"/> used to delete messages, with associated moderation logging.
        /// </summary>
        internal protected IModerationService ModerationService { get; }

        /// <summary>
        /// Check if time between posting 2 identical messages is lower, in seconds
        /// </summary>
        public double MinimumSecondsBetweenMessages { get; set; } = 5;

        /// <summary>
        /// Minimum message length to check
        /// </summary>
        public double MinimumMessageLength { get; set; } = 10;

        public DuplicitMessageHandler(ISelfUserProvider selfUserProvider, IModerationService moderationService)
        {
            SelfUserProvider = selfUserProvider;
            ModerationService = moderationService;
        }

        /// <inheritdoc />
        public Task HandleNotificationAsync(MessageReceivedNotification notification, CancellationToken cancellationToken = default)
            => CheckForDuplicitMessage(notification.Message);

        /// <inheritdoc />
        public Task HandleNotificationAsync(MessageDeletedNotification notification, CancellationToken cancellationToken = default)
            => RemoveLastMessageByUser(notification.Message.Value.Author);

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

            var selfUser = await SelfUserProvider.GetSelfUserAsync();
            if (author.Id == selfUser.Id)
                return;

            if (message.Content.Length < MinimumMessageLength)
                return;

            if (_messages.TryGetValue(author.Id, out var lastMessage))
            {
                _messages[author.Id] = new LastMessage(message);

                if (message.Content != lastMessage.Content)
                    return;

                var deltaSeconds = (message.Timestamp - lastMessage.Timestamp).TotalSeconds;
                if (deltaSeconds >= MinimumSecondsBetweenMessages)
                    return;
            }
            else
            {
                _messages.Add(author.Id, new LastMessage(message));
                return;
            }

            Log.Debug("Message {MessageId} is going to be deleted", message.Id);

            await ModerationService.DeleteMessageAsync(message, "Duplicit message detected", selfUser.Id);

            Log.Debug("Message {MessageId} was deleted because it was detected as duplicate", message.Id);

            await message.Channel.SendMessageAsync($"Sorry {author.Mention} your message has been removed - please don't post duplicit messages");
        }

        private Task RemoveLastMessageByUser(IUser user)
        {
            _messages.Remove(user.Id);
            return Task.CompletedTask;
        }

        private class LastMessage
        {
            public string Content { get; }
            public DateTimeOffset Timestamp { get; }

            public LastMessage(IMessage message)
            {
                Content = message.Content;
                Timestamp = message.Timestamp;
            }
        }
    }
}
