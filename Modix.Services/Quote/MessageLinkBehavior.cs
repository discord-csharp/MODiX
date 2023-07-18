using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Modix.Services.Quote
{
    public class MessageLinkBehavior : BehaviorBase
    {
        private static readonly Regex Pattern = new(
            @"^(?<Prelink>[\s\S]*?)?(?<OpenBrace><)?https?://(?:(?:ptb|canary)\.)?discord(app)?\.com/channels/(?<GuildId>\d+)/(?<ChannelId>\d+)/(?<MessageId>\d+)/?(?<CloseBrace>>)?(?<Postlink>[\s\S]*)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public MessageLinkBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            DiscordClient = discordClient;
            Log = serviceProvider.GetRequiredService<ILogger<MessageLinkBehavior>>();
        }

        private DiscordSocketClient DiscordClient { get; }

        private ILogger<MessageLinkBehavior> Log { get; }

        protected internal override Task OnStartingAsync()
        {
            DiscordClient.MessageReceived += OnMessageReceivedAsync;
            DiscordClient.MessageUpdated += OnMessageUpdatedAsync;

            return Task.CompletedTask;
        }

        protected internal override Task OnStoppedAsync()
        {
            DiscordClient.MessageReceived -= OnMessageReceivedAsync;
            DiscordClient.MessageUpdated -= OnMessageUpdatedAsync;

            return Task.CompletedTask;
        }

        private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> cached, SocketMessage message, ISocketMessageChannel channel)
        {
            var cachedMessage = await cached.GetOrDownloadAsync();

            if (Pattern.IsMatch(cachedMessage.Content))
                return;

            await OnMessageReceivedAsync(message);
        }

        private async Task OnMessageReceivedAsync(IMessage message)
        {
            if (message is not IUserMessage userMessage
                || userMessage.Author is not IGuildUser guildUser
                || guildUser.IsBot
                || guildUser.IsWebhook)
            {
                return;
            }

            foreach (Match match in Pattern.Matches(message.Content))
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
                        var channel = DiscordClient.GetChannel(channelId);

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

                            var success = await SendQuoteEmbedAsync(msg, userMessage);
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
                        Log.LogError(ex, "An error occurred while attempting to create a quote embed.");
                    }
                }
            }
        }

        /// <summary>
        ///     Creates a quote embed and sends the message to the same channel the message link is from.
        ///     Will also reply to the same message the <paramref name="source" /> is replying to.
        /// </summary>
        /// <param name="message">The message that will be quoted.</param>
        /// <param name="source">The message that contains the message link.</param>
        /// <returns>True when the the quote succeeds, otherwise False.</returns>
        private async Task<bool> SendQuoteEmbedAsync(IMessage message, IMessage source)
        {
            var success = false;
            await SelfExecuteRequest<IQuoteService>(async quoteService =>
            {
                await quoteService.BuildRemovableEmbed(message, source.Author,
                    async embed => //If embed building is unsuccessful, this won't execute
                    {
                        success = true;
                        return await source.Channel.SendMessageAsync(
                            embed: embed.Build(),
                            messageReference: source.Reference,
                            allowedMentions: AllowedMentions.None);
                    });
            });

            return success;
        }
    }
}
