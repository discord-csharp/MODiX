using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.Logging;
using Modix.Bot.Notifications;

namespace Modix.Bot.Responders.MessageQuotes;

public class MessageQuoteResponder(
    DiscordSocketClient discordSocketClient, MessageQuoteEmbedHelper messageQuoteEmbedHelper,
    ILogger<MessageQuoteResponder> logger)
    : INotificationHandler<MessageUpdatedNotificationV3>, INotificationHandler<MessageReceivedNotificationV3>
{
    private static readonly Regex _pattern = new(
        @"^(?<Prelink>[\s\S]*?)?(?<OpenBrace><)?https?://(?:(?:ptb|canary)\.)?discord(app)?\.com/channels/(?<GuildId>\d+)/(?<ChannelId>\d+)/(?<MessageId>\d+)/?(?<CloseBrace>>)?(?<Postlink>[\s\S]*)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public async Task Handle(MessageUpdatedNotificationV3 notification, CancellationToken cancellationToken)
    {
        var cachedMessage = await notification.OldMessage.GetOrDownloadAsync();

        if (_pattern.IsMatch(cachedMessage.Content))
            return;

        await OnMessageReceived(cachedMessage);
    }

    public async Task Handle(MessageReceivedNotificationV3 notification, CancellationToken cancellationToken)
    {
        await OnMessageReceived(notification.Message);
    }

    private async Task OnMessageReceived(IMessage message)
    {
        if (message is not IUserMessage userMessage
            || userMessage.Author is not IGuildUser guildUser
            || guildUser.IsBot
            || guildUser.IsWebhook)
        {
            return;
        }

        foreach (Match match in _pattern.Matches(message.Content))
        {
            // check if the link is surrounded with < and >. This was too annoying to do in regex
            if (match.Groups["OpenBrace"].Success && match.Groups["CloseBrace"].Success)
                continue;

            if (ulong.TryParse(match.Groups["GuildId"].Value, out var guildId)
                && ulong.TryParse(match.Groups["ChannelId"].Value, out var channelId)
                && ulong.TryParse(match.Groups["MessageId"].Value, out var messageId))
            {
                try
                {
                    var channel = discordSocketClient.GetChannel(channelId);

                    if (channel is ITextChannel { IsNsfw: true })
                    {
                        return;
                    }

                    if (channel is IGuildChannel guildChannel &&
                        channel is ISocketMessageChannel messageChannel)
                    {
                        var currentUser = await guildChannel.Guild.GetCurrentUserAsync();
                        var botChannelPermissions = currentUser.GetPermissions(guildChannel);
                        var userChannelPermissions = guildUser.GetPermissions(guildChannel);

                        if (!botChannelPermissions.ViewChannel || !userChannelPermissions.ViewChannel)
                        {
                            return;
                        }

                        var cacheMode = botChannelPermissions.ReadMessageHistory
                            ? CacheMode.AllowDownload
                            : CacheMode.CacheOnly;

                        var msg = await messageChannel.GetMessageAsync(messageId, cacheMode);

                        if (msg == null)
                            return;

                        var success = await TrySendQuoteEmbed(msg, userMessage);
                        if (success
                            && string.IsNullOrEmpty(match.Groups["Prelink"].Value)
                            && string.IsNullOrEmpty(match.Groups["Postlink"].Value))
                        {
                            await userMessage.DeleteAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while attempting to create a quote embed");
                }
            }
        }
    }

    private async Task<bool> TrySendQuoteEmbed(IMessage message, IMessage source)
    {
        var success = false;

        await messageQuoteEmbedHelper.BuildRemovableEmbed(message, source.Author,
            async embed => //If embed building is unsuccessful, this won't execute
            {
                success = true;
                return await source.Channel.SendMessageAsync(
                    embed: embed.Build(),
                    messageReference: source.Reference,
                    allowedMentions: AllowedMentions.None);
            });

        return success;
    }
}
