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
        private static readonly Regex Pattern = new Regex(
            @"(?<Prelink>\S+\s+\S*)?https?://(?:(?:ptb|canary)\.)?discordapp\.com/channels/(?<GuildId>\d+)/(?<ChannelId>\d+)/(?<MessageId>\d+)/?(?<Postlink>\S*\s+\S+)?",
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
            await OnMessageReceivedAsync(message);
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage)
                || !(userMessage.Author is SocketGuildUser guildUser)
                || guildUser.IsBot
                || guildUser.IsWebhook)
            {
                return;
            }

            foreach (Match match in Pattern.Matches(message.Content))
            {
                if (ulong.TryParse(match.Groups["GuildId"].Value, out var guildId)
                    && ulong.TryParse(match.Groups["ChannelId"].Value, out var channelId)
                    && ulong.TryParse(match.Groups["MessageId"].Value, out var messageId))
                {
                    try
                    {
                        var channel = DiscordClient.GetChannel(channelId);

                        if (channel is IGuildChannel &&
                            channel is ISocketMessageChannel messageChannel)
                        {
                            var msg = await messageChannel.GetMessageAsync(messageId);
                            if (msg == null) return;

                            var success = await SendQuoteEmbedAsync(msg, guildUser, userMessage.Channel);
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
                        Log.LogError(ex, "An error occured while attempting to create a quote embed.");
                    }
                }
            }
        }

        private async Task<bool> SendQuoteEmbedAsync(IMessage message, SocketGuildUser quoter, ISocketMessageChannel targetChannel)
        {
            bool success = false;
            await SelfExecuteRequest<IQuoteService>(async quoteService =>
            {
                await quoteService.BuildRemovableEmbed(message, quoter,
                    async (embed) => //If embed building is unsuccessful, this won't execute
                    {
                        success = true;
                        return await targetChannel.SendMessageAsync(embed: embed.Build());
                    });
            });

            return success;
        }
    }
}
