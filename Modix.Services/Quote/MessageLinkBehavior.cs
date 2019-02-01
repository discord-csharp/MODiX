using System;
using System.Collections.Generic;
using System.Linq;
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
                        if (await IsQuote(channelId, messageId)) { return; }

                        await SendQuoteEmbedAsync(channelId, messageId, guildUser, userMessage.Channel);
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex, "An error occured while attempting to create a quote embed.");
                    }
                }
            }
        }

        private async Task SendQuoteEmbedAsync(ulong originalChannelId, ulong messageId, SocketGuildUser quoter, ISocketMessageChannel targetChannel)
        {
            var channel = DiscordClient.GetChannel(originalChannelId);

            if (channel is IGuildChannel &&
                channel is ISocketMessageChannel messageChannel)
            {
                var msg = await messageChannel.GetMessageAsync(messageId);
                if (msg == null) { return; }

                await SelfExecuteRequest<IQuoteService>(async quoteService =>
                {
                    var embed = quoteService.BuildQuoteEmbed(msg, quoter)?.Build();
                    if (embed == null) { return; }

                    await targetChannel.SendMessageAsync(string.Empty, embed: embed);
                });
            }
        }

        private async Task<bool> IsQuote(ulong channelId, ulong messageId)
        {
            var foundMessage = await (DiscordClient.GetChannel(channelId) as ISocketMessageChannel)
                .GetMessageAsync(messageId);

            var hasQuoteField =
                foundMessage
                .Embeds?
                .SelectMany(d=>d.Fields)
                .Any(d => d.Name == "Quoted by");

            return hasQuoteField.HasValue && hasQuoteField.Value;
        }
    }
}
