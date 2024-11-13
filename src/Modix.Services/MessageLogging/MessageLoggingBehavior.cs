#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Modix.Common.Messaging;
using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.Utilities;

namespace Modix.Services.MessageLogging
{
    [ServiceBinding(ServiceLifetime.Scoped)]
    public class MessageLoggingBehavior
        : INotificationHandler<MessageDeletedNotification>,
            INotificationHandler<MessageUpdatedNotification>
    {
        public MessageLoggingBehavior(
            DesignatedChannelService designatedChannelService,
            DiscordSocketClient discordSocketClient,
            ILogger<MessageLoggingBehavior> logger)
        {
            _designatedChannelService = designatedChannelService;
            _discordSocketClient = discordSocketClient;
            _logger = logger;
        }

        public async Task HandleNotificationAsync(
            MessageDeletedNotification notification,
            CancellationToken cancellationToken)
        {
            var channel = await notification.Channel.GetOrDownloadAsync();
            var guild = (channel as IGuildChannel)?.Guild;

            using var logScope = MessageLoggingLogMessages.BeginMessageNotificationScope(_logger, guild?.Id, notification.Channel.Id, notification.Message.Id);

            MessageLoggingLogMessages.MessageDeletedHandling(_logger);

            await TryLogAsync(
                guild,
                notification.Message.HasValue ? notification.Message.Value : null,
                null,
                channel,
                () =>
                {
                    var fields = Enumerable.Empty<EmbedFieldBuilder>()
                        .Concat(FormatMessageContent(notification.Message.HasValue
                                ? notification.Message.Value.Content
                                : null)
                            .EnumerateLongTextAsFieldBuilders("**Content**"))
                        .Append(new EmbedFieldBuilder()
                            .WithName("Channel ID")
                            .WithValue(notification.Channel.Id)
                            .WithIsInline(true))
                        .Append(new EmbedFieldBuilder()
                            .WithName("Message ID")
                            .WithValue(notification.Message.Id)
                            .WithIsInline(true));

                    var embedBuilder = new EmbedBuilder();

                    if (notification.Message.HasValue)
                    {
                        embedBuilder = embedBuilder
                            .WithUserAsAuthor(notification.Message.Value.Author, notification.Message.Value.Author.Id.ToString());

                        if (notification.Message.Value.Attachments.Any())
                            fields = fields
                                .Append(new EmbedFieldBuilder()
                                    .WithName("Attachments")
                                    .WithValue(string.Join(", ", notification.Message.Value.Attachments.Select(attachment => $"{attachment.Filename} ({attachment.Size}b)"))));
                    }

                    return embedBuilder
                        //                   ↓ This character is a "wastebasket", don't worry
                        .WithDescription($"\\🗑️ **Message deleted in {MentionUtils.MentionChannel(notification.Channel.Id)}**")
                        .WithCurrentTimestamp()
                        .WithFields(fields)
                        .Build();
                },
                cancellationToken);

            MessageLoggingLogMessages.MessageDeletedHandled(_logger);
        }

        public async Task HandleNotificationAsync(
            MessageUpdatedNotification notification,
            CancellationToken cancellationToken)
        {
            var guild = (notification.Channel as SocketGuildChannel)?.Guild;
            if(notification.NewMessage.Author.Id == default)
            {
                // update caused by new thread created or deleted
                return;
            }

            using var logScope = MessageLoggingLogMessages.BeginMessageNotificationScope(_logger, guild?.Id, notification.Channel.Id, notification.NewMessage.Id);

            MessageLoggingLogMessages.MessageUpdatedHandling(_logger);

            await TryLogAsync(
                guild,
                notification.OldMessage.HasValue ? notification.OldMessage.Value : null,
                notification.NewMessage,
                notification.Channel,
                () => new EmbedBuilder()
                    .WithUserAsAuthor(notification.NewMessage.Author, notification.NewMessage.Author.Id.ToString())
                    .WithDescription($"\\📝 **Message edited in {notification.NewMessage.GetJumpUrlForEmbed()}**")
                    .WithCurrentTimestamp()
                    .WithFields(Enumerable.Empty<EmbedFieldBuilder>()
                        .Concat(FormatMessageContent(notification.OldMessage.HasValue
                                ? notification.OldMessage.Value.Content
                                : null)
                            .EnumerateLongTextAsFieldBuilders("**Original**"))
                        .Concat(FormatMessageContent(notification.NewMessage.Content)
                            .EnumerateLongTextAsFieldBuilders("**Updated**"))
                        .Append(new EmbedFieldBuilder()
                            .WithName("Channel ID")
                            .WithValue(notification.Channel.Id)
                            .WithIsInline(true))
                        .Append(new EmbedFieldBuilder()
                            .WithName("Message ID")
                            .WithValue(notification.NewMessage.Id)
                            .WithIsInline(true)))
                    .Build(),
                cancellationToken);

            MessageLoggingLogMessages.MessageUpdatedHandled(_logger);
        }

        private static string FormatMessageContent(
                string? messageContent)
            => string.IsNullOrWhiteSpace(messageContent)
                ? "[N/A]"
                // Escape backticks to preserve formatting (zero-width spaces are quite useful)
                : messageContent.Replace("```", '\u200B' + "`" + '\u200B' + "`" + '\u200B' + "`" + '\u200B');

        private async Task TryLogAsync(
            IGuild? guild,
            IMessage? oldMessage,
            IMessage? newMessage,
            IMessageChannel channel,
            Func<Embed> renderLogMessage,
            CancellationToken cancellationToken)
        {
            if (guild is null)
            {
                MessageLoggingLogMessages.IgnoringNonGuildMessage(_logger);
                return;
            }

            // I.E. we have content for both messages and can see for sure it hasn't changed, E.G. Embed changes
            if ((oldMessage?.Content == newMessage?.Content)
                && (oldMessage is { })
                && (newMessage is { }))
            {
                MessageLoggingLogMessages.IgnoringUnchangedMessage(_logger);
                return;
            }

            var selfUser = _discordSocketClient.CurrentUser;
            MessageLoggingLogMessages.SelfUserFetched(_logger, selfUser.Id);

            if ((oldMessage?.Author ?? newMessage?.Author)?.Id == selfUser.Id)
            {
                MessageLoggingLogMessages.IgnoringSelfAuthoredMessage(_logger);
                return;
            }

            var channelIsUnmoderated = await _designatedChannelService.ChannelHasDesignation(
                guild.Id,
                channel.Id,
                DesignatedChannelType.Unmoderated,
                cancellationToken);
            if (channelIsUnmoderated)
            {
                MessageLoggingLogMessages.IgnoringUnmoderatedChannel(_logger);
                return;
            }
            MessageLoggingLogMessages.ModeratedChannelIdentified(_logger);

            var messageLogChannels = await _designatedChannelService.GetDesignatedChannels(
                guild,
                DesignatedChannelType.MessageLog);
            if (messageLogChannels.Count == 0)
            {
                MessageLoggingLogMessages.MessageLogChannelsNotFound(_logger);
                return;
            }
            MessageLoggingLogMessages.MessageLogChannelsFetched(_logger, messageLogChannels.Count);

            var embed = renderLogMessage.Invoke();

            foreach (var messageLogChannel in messageLogChannels)
            {
                MessageLoggingLogMessages.MessageLogMessageSending(_logger, messageLogChannel.Id);
                var logMessage = await messageLogChannel.SendMessageAsync(string.Empty, false, embed);
                MessageLoggingLogMessages.MessageLogMessageSent(_logger, messageLogChannel.Id, logMessage.Id);
            }
        }

        private readonly DesignatedChannelService _designatedChannelService;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly ILogger _logger;
    }
}
