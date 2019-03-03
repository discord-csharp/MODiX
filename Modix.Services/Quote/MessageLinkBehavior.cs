using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modix.Services.AutoRemoveMessage;

namespace Modix.Services.Quote
{
    public class MessageLinkBehavior : BehaviorBase
    {
        private static readonly Regex Pattern = new Regex(
            @"https?://(?:(?:ptb|canary)\.)?discordapp\.com/channels/(?<GuildId>\d+)/(?<ChannelId>\d+)/(?<MessageId>\d+)",
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

            return Task.CompletedTask;
        }

        protected internal override Task OnStoppedAsync()
        {
            DiscordClient.MessageReceived -= OnMessageReceivedAsync;

            return Task.CompletedTask;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage) ||
                !(userMessage.Author is SocketGuildUser guildUser) ||
                guildUser.IsBot || guildUser.IsWebhook)
            {
                return;
            }

            foreach (Match match in Pattern.Matches(message.Content))
            {
                if (ulong.TryParse(match.Groups["GuildId"].Value, out var guildId) &&
                    ulong.TryParse(match.Groups["ChannelId"].Value, out var channelId) &&
                    ulong.TryParse(match.Groups["MessageId"].Value, out var messageId))
                {
                    try
                    {
                        var channel = DiscordClient.GetChannel(channelId);

                        if (channel is IGuildChannel &&
                            channel is ISocketMessageChannel messageChannel)
                        {
                            var msg = await messageChannel.GetMessageAsync(messageId);
                            if (msg == null || await IsQuote(msg))
                                return;

                            await SendQuoteEmbedAsync(msg, guildUser, userMessage.Channel);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex, "An error occured while attempting to create a quote embed.");
                    }
                }
            }
        }

        private async Task SendQuoteEmbedAsync(IMessage message, SocketGuildUser quoter, ISocketMessageChannel targetChannel)
        {
            await SelfExecuteRequest<IQuoteService>(async quoteService =>
            {
                var embed = quoteService.BuildQuoteEmbed(message, quoter);
                if (embed == null) return;

                embed.Fields.First(d => d.Name == "Quoted by")
                .Value += $" from **[#{message.Channel.Name}]({message.GetJumpUrl()})**";

                embed.WithFooter("React with ❌ to remove this embed.");
                embed.WithTimestamp(message.Timestamp);

                var removableMessage = await targetChannel.SendMessageAsync(string.Empty, embed: embed.Build());
                await SelfExecuteRequest<IAutoRemoveMessageService>(async messageRemoveService
                    => await messageRemoveService.RegisterRemovableMessageAsync(removableMessage, quoter));
            });
        }

        private async Task<bool> IsQuote(IMessage message)
        {
            var hasQuoteField =
                message
                .Embeds?
                .SelectMany(d=>d.Fields)
                .Any(d => d.Name == "Quoted by");

            return hasQuoteField.HasValue && hasQuoteField.Value;
        }
    }
}
