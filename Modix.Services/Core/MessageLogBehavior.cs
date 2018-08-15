using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Data.Models.Core;
using Modix.Services.Utilities;
using Serilog;

namespace Modix.Services.Core
{
    public class MessageLogBehavior : BehaviorBase
    {
        private readonly DiscordSocketClient _discordClient;

        public MessageLogBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _discordClient = discordClient;
        }

        protected internal override Task OnStartingAsync()
        {
            _discordClient.MessageDeleted += HandleMessageDelete;
            _discordClient.MessageUpdated += HandleMessageEdit;

            return Task.CompletedTask;
        }

        protected internal override Task OnStoppedAsync()
        {
            _discordClient.MessageDeleted -= HandleMessageDelete;
            _discordClient.MessageUpdated -= HandleMessageEdit;

            return Task.CompletedTask;
        }

        private async Task HandleMessageEdit(Cacheable<IMessage, ulong> cachedOriginal, SocketMessage updated, ISocketMessageChannel channel)
        {
            var guild = (channel as SocketGuildChannel)?.Guild;

            if (guild == null)
            {
                Log.Information("Recieved message update event for non-guild message, ignoring");
            }

            var original = await cachedOriginal.GetOrDownloadAsync();

            //Skip things like embed updates
            if (original.Content == updated.Content) { return; }

            var embed = new EmbedBuilder()
                .WithVerboseAuthor(original.Author)
                .WithDescription($"**Original**```{original.Content}```\n**Updated**```{updated.Content}```")
                .WithTimestamp(DateTimeOffset.UtcNow);

            await SelfExecuteRequest<IDesignatedChannelService>(async designatedChannelService =>
            {
                await designatedChannelService.SendToDesignatedChannelsAsync(
                    guild, ChannelDesignation.MessageLog,
                    $":pencil:Message Edited in {MentionUtils.MentionChannel(channel.Id)}", embed.Build());
            });
        }

        private async Task HandleMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var guild = (channel as SocketGuildChannel)?.Guild;

            if (guild == null)
            {
                Log.Information("Recieved message update event for non-guild message, ignoring");
            }

            var embed = new EmbedBuilder();

            if (!message.HasValue)
            {
                embed = embed.WithDescription($"**Content**```Unknown, message not cached```");
            }
            else
            {
                var cached = message.Value;

                //Don't log messages Modix deletes
                if (message.Value.Author.Id == _discordClient.CurrentUser.Id) { return; }

                embed = embed
                    .WithVerboseAuthor(cached.Author)
                    .WithDescription($"**Content**```{cached.Content}```");
            }

            embed = embed.WithTimestamp(DateTimeOffset.UtcNow);

            await SelfExecuteRequest<IDesignatedChannelService>(async designatedChannelService =>
            {
                await designatedChannelService.SendToDesignatedChannelsAsync(
                    guild, ChannelDesignation.MessageLog,
                    $":wastebasket:Message Deleted in {MentionUtils.MentionChannel(channel.Id)} `{message.Id}`", embed.Build());
            });
        }
    }
}
