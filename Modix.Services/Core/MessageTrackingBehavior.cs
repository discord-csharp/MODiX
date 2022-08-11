#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Discord;
using StatsdClient;

using Modix.Common.Messaging;
using Modix.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Modix.Services.Core
{
    [ServiceBinding(ServiceLifetime.Scoped)]
    public class MessageTrackingBehavior
        : INotificationHandler<MessageDeletedNotification>,
            INotificationHandler<MessageReceivedNotification>
    {
        public MessageTrackingBehavior(
            ICommandPrefixParser commandPrefixParser,
            IDogStatsd dogStatsd,
            ILogger<MessageTrackingBehavior> logger,
            IMessageRepository messageRepository)
        {
            _commandPrefixParser = commandPrefixParser;
            _dogStatsd = dogStatsd;
            _logger = logger;
            _messageRepository = messageRepository;
        }

        public async Task HandleNotificationAsync(
            MessageDeletedNotification notification,
            CancellationToken cancellationToken)
        {
            var message = notification.Message.HasValue ? notification.Message.Value : null;
            var channel = await notification.Channel.GetOrDownloadAsync();
            var guild = (channel as IGuildChannel)?.Guild;

            using var logScope = MessageLogMessages.BeginMessageNotificationScope(_logger, guild?.Id, notification.Message.Id, channel.Id);

            MessageLogMessages.MessageDeletedHandling(_logger);

            await TryTrackMessageAsync(
                guild,
                message,
                async (_) =>
                {
                    MessageLogMessages.MessageRecordDeleting(_logger);
                    await _messageRepository.DeleteAsync(notification.Message.Id);
                    MessageLogMessages.MessageRecordDeleted(_logger);
                },
                cancellationToken);

            MessageLogMessages.MessageDeletedHandled(_logger);
        }

        public async Task HandleNotificationAsync(
            MessageReceivedNotification notification,
            CancellationToken cancellationToken)
        {
            using var statsScope = _dogStatsd.StartTimer("message_processing_ms");

            var message = notification.Message;
            var channel = notification.Message.Channel;
            var guild = (channel as IGuildChannel)?.Guild;

            using var logScope = MessageLogMessages.BeginMessageNotificationScope(_logger, guild?.Id, message.Id, channel.Id);

            MessageLogMessages.MessageReceivedHandling(_logger);

            await TryTrackMessageAsync(
                guild,
                notification.Message,
                async (guildId) =>
                {
                    MessageLogMessages.MessageRecordCreating(_logger);
                    await _messageRepository.CreateAsync(new MessageCreationData()
                    {
                        Id          = message.Id,
                        GuildId     = guildId,
                        ChannelId   = channel.Id,
                        AuthorId    = message.Author.Id,
                        Timestamp   = message.Timestamp
                    });
                    MessageLogMessages.MessageRecordCreated(_logger);
                },
                cancellationToken);

            MessageLogMessages.MessageReceivedHandled(_logger);
        }

        private async Task TryTrackMessageAsync(
            IGuild? guild,
            IMessage? message,
            Func<ulong, Task> asyncTrackAction,
            CancellationToken cancellationToken)
        {
            if (guild is null)
            {
                MessageLogMessages.IgnoringNonGuildMessage(_logger);
                return;
            }

            if (message is not null)
            {
                if (message is not IUserMessage userMessage || message.Author.IsBot || message.Author.IsWebhook)
                {
                    MessageLogMessages.IgnoringNonHumanMessage(_logger);
                    return;
                }

                if ((await _commandPrefixParser.TryFindCommandArgPosAsync(userMessage, cancellationToken)).HasValue)
                {
                    MessageLogMessages.IgnoringCommandMessage(_logger);
                    return;
                }
            }

            MessageLogMessages.TransactionBeginning(_logger);
            using var transaction = await _messageRepository.BeginMaintainTransactionAsync();

            await asyncTrackAction.Invoke(guild.Id);

            transaction.Commit();
            MessageLogMessages.TransactionCommitted(_logger);
        }

        private readonly ICommandPrefixParser _commandPrefixParser;
        private readonly IDogStatsd _dogStatsd;
        private readonly ILogger _logger;
        private readonly IMessageRepository _messageRepository;
    }
}
