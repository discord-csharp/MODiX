using System;
using System.Collections.Generic;
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
            @"https?://(canary\.)?discordapp\.com/channels/(?<GuildId>\d+)/(?<ChannelId>\d+)/(?<MessageId>\d+)",
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
                guildUser.IsBot ||
                string.IsNullOrWhiteSpace(userMessage.Content))
            {
                return;
            }

            var embeds = new List<EmbedBuilder>();

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

                            if (msg != null)
                            {
                                await SelfExecuteRequest<IQuoteService>(
                                    quoteService =>
                                    {
                                        var embed = quoteService.BuildQuoteEmbed(msg, guildUser);
                                        embeds.Add(embed);
                                        return Task.CompletedTask;
                                    });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex, "An error occured while attempting to create a quote embed.");
                    }
                }
            }

            foreach (var embed in embeds)
            {
                await userMessage.Channel.SendMessageAsync(string.Empty, embed: embed);
            }
        }
    }
}
