using System;
using System.Linq;
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

        
        private string FormatMessage(string input)
        {
            //Escape backticks to preserve formatting (zero-width spaces are quite useful)
            input = input.Replace("```", '\u200B' + "`" + '\u200B' + "`" + '\u200B' + "`" + '\u200B');

            //If the message content is empty, return a notice rather than nothing
            input = string.IsNullOrWhiteSpace(input) ? "Empty Message Content" : input;

            return input;
        }

        /// <summary>
        /// Determines whether or not to skip a message event, based on unmoderated channel designations
        /// </summary>
        /// <param name="guild">The guild designations should be looked up for</param>
        /// <param name="channel">The channel designations should be looked up for</param>
        /// <returns>True if the channel is designated as Unmoderated, false if not</returns>
        private async Task<bool> ShouldSkip(IGuild guild, IMessageChannel channel)
        {
            bool result = false;

            await SelfExecuteRequest<IDesignatedChannelService>(async designatedChannelService =>
            {
                result = await designatedChannelService.ChannelHasDesignationAsync(guild, channel, DesignatedChannelType.Unmoderated);
            });

            return result;
        }

        private async Task HandleMessageEdit(Cacheable<IMessage, ulong> cachedOriginal, SocketMessage updated, ISocketMessageChannel channel)
        {
            //Don't log when Modix edits its own messages
            if (updated.Author.Id == _discordClient.CurrentUser.Id) { return; }

            var guild = (channel as SocketGuildChannel)?.Guild;

            if (guild == null)
            {
                Log.Information("Recieved message update event for non-guild message, ignoring");
                return;
            }

            if (await ShouldSkip(guild, channel)) { return; }

            var original = await cachedOriginal.GetOrDownloadAsync();

            //Skip things like embed updates
            if (original.Content == updated.Content) { return; }

            string descriptionText = $"**[Original]({original.GetMessageLink()})**\n```{FormatMessage(original.Content)}```";

            if (descriptionText.Length <= 2048)
            {
                descriptionText += $"\n**Updated**\n```{FormatMessage(updated.Content)}```"; ;
            }

            var embed = new EmbedBuilder()
                .WithVerboseAuthor(original.Author)
                .WithDescription(descriptionText)
                .WithCurrentTimestamp();

            await SelfExecuteRequest<IDesignatedChannelService>(async designatedChannelService =>
            {
                await designatedChannelService.SendToDesignatedChannelsAsync(
                    guild, DesignatedChannelType.MessageLog,
                    $":pencil:Message Edited in {MentionUtils.MentionChannel(channel.Id)}", embed.Build());
            });
        }

        private async Task HandleMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var guild = (channel as SocketGuildChannel)?.Guild;

            if (guild == null)
            {
                Log.Information("Recieved message update event for non-guild message, ignoring");
                return;
            }

            if (await ShouldSkip(guild, channel)) { return; }

            var embed = new EmbedBuilder();
            string descriptionContent = $"**Content**\n```Unknown, message not cached```";

            if (message.HasValue)
            {
                var cached = message.Value;

                //Don't log when messages from Modix are deleted
                if (message.Value.Author.Id == _discordClient.CurrentUser.Id) { return; }

                descriptionContent = $"**Content**\n```{FormatMessage(cached.Content)}```";

                embed = embed
                    .WithVerboseAuthor(cached.Author);

                if (cached.Attachments.Any())
                {
                    embed = embed.AddField
                    (
                        field => field
                            .WithName("Attachments")
                            .WithValue(string.Join(", ", cached.Attachments.Select(d => $"{d.Filename} ({d.Size}b)")))
                    );
                }
            }

            embed = embed
                .WithDescription(descriptionContent)
                .WithCurrentTimestamp();

            await SelfExecuteRequest<IDesignatedChannelService>(async designatedChannelService =>
            {
                await designatedChannelService.SendToDesignatedChannelsAsync(
                    guild, DesignatedChannelType.MessageLog,
                    $":wastebasket:Message Deleted in {MentionUtils.MentionChannel(channel.Id)} `{message.Id}`", embed.Build());
            });
        }
    }
}
