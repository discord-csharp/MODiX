using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Modix.Data.Repositories;
using Modix.RemoraShim.Services;
using Modix.Services.Core;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using StatsdClient;

namespace Modix.RemoraShim.Behaviors
{
    public class TrackingHandler : IResponder<IMessageCreate>, IResponder<IMessageDelete>, IResponder<IThreadCreate>
    {
        private readonly IDogStatsd _dogStatsd;
        private readonly ILogger<TrackingHandler> _logger;
        private readonly IMessageRepository _messageRepository;
        private readonly IChannelService _channelService;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IThreadService _threadService;

        public TrackingHandler(
            IDogStatsd dogStatsd,
            ILogger<TrackingHandler> logger,
            IMessageRepository messageRepository,
            IChannelService channelService,
            IDiscordRestChannelAPI channelApi,
            IThreadService threadSvc
            )
        {
            _dogStatsd = dogStatsd;
            _logger = logger;
            _messageRepository = messageRepository;
            _channelService = channelService;
            _channelApi = channelApi;
            _threadService = threadSvc;
        }

        public async Task<Result> RespondAsync(IMessageCreate message, CancellationToken ct = default)
        {

            await HandleNotificationAsync(message, ct);
            return Result.FromSuccess();
        }

        public async Task<Result> RespondAsync(IMessageDelete gatewayEvent, CancellationToken ct = default)
        {
            await HandleNotificationAsync(gatewayEvent, ct);
            return Result.FromSuccess();
        }

        public async Task<Result> RespondAsync(IThreadCreate threadCreate, CancellationToken ct = default)
        {
            await _channelService.TrackChannelAsync(threadCreate.Name.Value, threadCreate.ID.Value, threadCreate.GuildID.Value.Value, ct);

            return Result.FromSuccess();
        }

        public async Task HandleNotificationAsync(IMessageDelete notification, CancellationToken ct)
        {
            var message = notification;
            var channelId = notification.ChannelID;
            var guild = notification.GuildID;

            var isThreadChannel = await _threadService.IsThreadChannelAsync(channelId, ct);
            if (!isThreadChannel)
            {
                return;
            }

            using var logScope = MessageLogMessages.BeginMessageNotificationScope(_logger, guild.Value.Value, channelId.Value, notification.ID.Value);

            MessageLogMessages.MessageDeletedHandling(_logger);

            await TryTrackMessageAsync(
                guild,
                null,
                async (_) =>
                {
                    MessageLogMessages.MessageRecordDeleting(_logger);
                    await _messageRepository.DeleteAsync(notification.ID.Value);
                    MessageLogMessages.MessageRecordDeleted(_logger);
                });

            MessageLogMessages.MessageDeletedHandled(_logger);
        }

        public async Task HandleNotificationAsync(IMessageCreate notification, CancellationToken ct)
        {
            var isThreadChannel = await _threadService.IsThreadChannelAsync(notification.ChannelID, ct);
            if (!isThreadChannel)
            {
                return;
            }

            using var statsScope = _dogStatsd.StartTimer("message_processing_ms");

            var message = notification;
            var channel = notification.ChannelID;
            var guild = notification.GuildID;

            using var logScope = MessageLogMessages.BeginMessageNotificationScope(_logger, guild.Value.Value, channel.Value, notification.ID.Value);

            MessageLogMessages.MessageReceivedHandling(_logger);

            await TryTrackMessageAsync(
                guild,
                message,
                async (guildId) =>
                {
                    MessageLogMessages.MessageRecordCreating(_logger);
                    await _messageRepository.CreateAsync(new MessageCreationData()
                    {
                        Id = message.ID.Value,
                        GuildId = guildId,
                        ChannelId = channel.Value,
                        AuthorId = message.Author.ID.Value,
                        Timestamp = message.Timestamp
                    });
                    MessageLogMessages.MessageRecordCreated(_logger);
                });

            MessageLogMessages.MessageReceivedHandled(_logger);
        }

        private async Task TryTrackMessageAsync(
            Optional<Snowflake> guild,
            IMessageCreate? message,
            Func<ulong, Task> asyncTrackAction)
        {
            if (!guild.HasValue)
            {
                MessageLogMessages.IgnoringNonGuildMessage(_logger);
                return;
            }

            if (message != null)
            {
                var author = message.Author;
                if ((author.IsBot.HasValue && author.IsBot.Value) || message.WebhookID.HasValue)
                {
                    MessageLogMessages.IgnoringNonHumanMessage(_logger);
                    return;
                }

                if (message.Content.StartsWith("!"))
                {
                    MessageLogMessages.IgnoringCommandMessage(_logger);
                    return;
                }
            }

            MessageLogMessages.TransactionBeginning(_logger);
            using var transaction = await _messageRepository.BeginMaintainTransactionAsync();

            await asyncTrackAction.Invoke(guild.Value.Value);

            transaction.Commit();
            MessageLogMessages.TransactionCommitted(_logger);
        }
    }
}
