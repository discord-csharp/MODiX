using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Modix.Data.Models.Core;
using Modix.RemoraShim.Services;
using Modix.Services.Core;
using Modix.Services.MessageLogging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Modix.RemoraShim.Behaviors
{
    public class MessageLoggingBehavior : IResponder<IMessageDelete>//, IResponder<IMessageUpdate>
    {
        private readonly ILogger<MessageLoggingBehavior> _logger;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IThreadService _threadSvc;

        public MessageLoggingBehavior(ILogger<MessageLoggingBehavior> logger,
            IDiscordRestUserAPI userApi,
            IDiscordRestChannelAPI channelApi,
            IDesignatedChannelService designatedChannelService,
            IThreadService threadService)
        {
            _logger = logger;
            _userApi = userApi;
            _channelApi = channelApi;
            _designatedChannelService = designatedChannelService;
            _threadSvc = threadService;
        }

        public async Task<Result> RespondAsync(IMessageDelete gatewayEvent, CancellationToken ct = default)
        {
            await HandleNotificationAsync(gatewayEvent, ct);
            return Result.FromSuccess();
        }

        public async Task<Result> RespondAsync(IMessageUpdate gatewayEvent, CancellationToken ct = default)
        {
            await HandleNotificationAsync(gatewayEvent, ct);
            return Result.FromSuccess();
        }


        private async Task HandleNotificationAsync(IMessageDelete notification, CancellationToken cancellationToken)
        {
            if (!notification.GuildID.HasValue)
            {
                return;
            }

            var channelId = notification.ChannelID;

            var isThreadChannel = await _threadSvc.IsThreadChannelAsync(channelId, cancellationToken);
            if(!isThreadChannel)
            {
                return;
            }

            var guild = notification.GuildID;

            using var logScope = MessageLoggingLogMessages.BeginMessageNotificationScope(_logger, guild.Value.Value, notification.ChannelID.Value, notification.ID.Value);

            MessageLoggingLogMessages.MessageDeletedHandling(_logger);

            await TryLogAsync(
                guild,
                null,
                null,
                notification.ChannelID,
                () =>
                {
                    var fields = Enumerable.Empty<IEmbedField>()
                        .Concat(FormatMessageContent(null)
                            .EnumerateLongTextAsFieldBuilders("**Content**"))
                        .Append(new EmbedField("Channel ID", notification.ChannelID.Value.ToString(), true))
                        .Append(new EmbedField("Message ID", notification.ID.Value.ToString(), true));
                    //                                ↓ This character is a "wastebasket", don't worry
                    return new Embed(Description: $"\\🗑️ **Message deleted in <#{notification.ChannelID.Value}>**",
                        Footer: new EmbedFooter("", default, default),
                        Fields: new Optional<IReadOnlyList<IEmbedField>>(fields.ToList().AsReadOnly()));
                },
                cancellationToken);

            MessageLoggingLogMessages.MessageDeletedHandled(_logger);
        }

        public async Task HandleNotificationAsync(IPartialMessage notification, CancellationToken cancellationToken)
        {
            if (!notification.GuildID.HasValue)
            {
                return;
            }

            var channelId = notification.ChannelID.Value;

            var isThreadChannel = await _threadSvc.IsThreadChannelAsync(channelId, cancellationToken);
            if (!isThreadChannel)
            {
                return;
            }

            var guild = notification.GuildID;

            using var logScope = MessageLoggingLogMessages.BeginMessageNotificationScope(_logger, guild.Value.Value, notification.ChannelID.Value.Value, notification.ID.Value.Value);

            MessageLoggingLogMessages.MessageUpdatedHandling(_logger);
            var author = notification.Author.HasValue ? notification.Author.Value : null;
            await TryLogAsync(
                guild,
                notification.ReferencedMessage.HasValue ? new Optional<IPartialMessage>(notification.ReferencedMessage.Value!.ToPartialMessage()) : new Optional<IPartialMessage>(),
                new Optional<IPartialMessage>(notification),
                notification.ChannelID.Value,
                () => new Embed(
                    Author: author.WithUserAsAuthor(author?.ID.Value.ToString()),
                    Description: $"\\📝 **Message edited in {notification.GetJumpUrlForEmbed()}**",
                    Timestamp: DateTimeOffset.UtcNow,
                    Fields: Enumerable.Empty<IEmbedField>()
                        .Concat(FormatMessageContent(
                            notification.Content.HasValue ? notification.Content.Value : null)
                            .EnumerateLongTextAsFieldBuilders("**Original**", false))
                        .Concat(FormatMessageContent(
                            notification.ReferencedMessage.HasValue ? notification.ReferencedMessage.Value?.Content : null)
                            .EnumerateLongTextAsFieldBuilders("**Updated**", false))
                        .Append(new EmbedField("Channel ID", notification.ChannelID.Value.Value.ToString(), true))
                        .Append(new EmbedField("Message ID", notification.ID.Value.ToString(), true)).ToList()),

                cancellationToken);

            MessageLoggingLogMessages.MessageUpdatedHandled(_logger);
        }

        private static string FormatMessageContent(string? messageContent)
            => string.IsNullOrWhiteSpace(messageContent)
                ? "[N/A]"
                // Escape backticks to preserve formatting (zero-width spaces are quite useful)
                : messageContent.Replace("```", '\u200B' + "`" + '\u200B' + "`" + '\u200B' + "`" + '\u200B');

        private async Task TryLogAsync(
            Optional<Snowflake> guildId,
            Optional<IPartialMessage>? oldMessage,
            Optional<IPartialMessage>? newMessage,
            Snowflake channel,
            Func<IEmbed> renderLogMessage,
            CancellationToken cancellationToken)
        {
            if (!guildId.HasValue)
            {
                MessageLoggingLogMessages.IgnoringNonGuildMessage(_logger);
                return;
            }

            if (oldMessage.HasValue && newMessage.HasValue) // both are null on delete
            {
                // I.E. we have content for both messages and can see for sure it hasn't changed, E.G. Embed changes
                if (oldMessage.GetOptionalValueOrDefault()?.Content == newMessage.GetOptionalValueOrDefault()?.Content)
                {
                    MessageLoggingLogMessages.IgnoringUnchangedMessage(_logger);
                    return;
                }
            }

            var selfUser = await _userApi.GetCurrentUserAsync(cancellationToken);
            MessageLoggingLogMessages.SelfUserFetched(_logger, selfUser.Entity!.ID.Value);

            if ((oldMessage?.Value.Author ?? newMessage?.Value.Author)?.Value.ID == selfUser.Entity.ID)
            {
                MessageLoggingLogMessages.IgnoringSelfAuthoredMessage(_logger);
                return;
            }

            var channelIsUnmoderated = await _designatedChannelService.ChannelHasDesignationAsync(
                guildId.Value.Value,
                channel.Value,
                DesignatedChannelType.Unmoderated,
                cancellationToken);
            if (channelIsUnmoderated)
            {
                MessageLoggingLogMessages.IgnoringUnmoderatedChannel(_logger);
                return;
            }
            MessageLoggingLogMessages.ModeratedChannelIdentified(_logger);

            var messageLogChannels = await _designatedChannelService.GetDesignatedChannelsAsync(guildId.Value.Value);
            messageLogChannels = messageLogChannels.Where(a => a.Type == DesignatedChannelType.MessageLog).ToList().AsReadOnly();
            if (messageLogChannels.Count == 0)
            {
                MessageLoggingLogMessages.MessageLogChannelsNotFound(_logger);
                return;
            }
            MessageLoggingLogMessages.MessageLogChannelsFetched(_logger, messageLogChannels.Count);

            var embed = renderLogMessage.Invoke();

            foreach (var messageLogChannel in messageLogChannels)
            {
                MessageLoggingLogMessages.MessageLogMessageSending(_logger, messageLogChannel.Channel.Id);
                var logMessage = await _channelApi.CreateMessageAsync(new Snowflake(messageLogChannel.Channel.Id), embeds: new Optional<IReadOnlyList<IEmbed>>(new IEmbed[] { embed }));
                if (logMessage.IsSuccess)
                {
                    MessageLoggingLogMessages.MessageLogMessageSent(_logger, messageLogChannel.Channel.Id, logMessage.Entity.ID.Value);
                }
            }
        }
    }
}
