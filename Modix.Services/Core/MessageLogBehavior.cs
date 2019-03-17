using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Modix.Data.Models.Core;
using Modix.Data.Repositories;
using Modix.Services.Utilities;

namespace Modix.Services.Core
{
    public class MessageLogBehavior : BehaviorBase
    {
        private readonly DiscordSocketClient _discordClient;

        public MessageLogBehavior(DiscordSocketClient discordClient, ILogger<MessageLogBehavior> logger, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _discordClient = discordClient;

            Log = logger;
        }

        private ILogger<MessageLogBehavior> Log { get; }

        internal protected override Task OnStartingAsync()
        {
            _discordClient.MessageReceived += HandleMessageReceived;
            _discordClient.MessageDeleted += HandleMessageDelete;
            _discordClient.MessageUpdated += HandleMessageEdit;

            return Task.CompletedTask;
        }

        internal protected override Task OnStoppedAsync()
        {
            _discordClient.MessageReceived -= HandleMessageReceived;
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

        private async Task TryLog(IGuild guild, string content, Embed embed)
        {
            await SelfExecuteRequest<IDesignatedChannelService>(async designatedChannelService =>
            {
                if (!await designatedChannelService.AnyDesignatedChannelAsync(guild.Id, DesignatedChannelType.MessageLog))
                {
                    return;
                }

                await designatedChannelService.SendToDesignatedChannelsAsync(guild, DesignatedChannelType.MessageLog, content, embed);
            });
        }

        /// <summary>
        /// Determines whether or not to skip a message event, based on unmoderated channel designations
        /// </summary>
        /// <param name="guild">The guild designations should be looked up for</param>
        /// <param name="channel">The channel designations should be looked up for</param>
        /// <returns>True if the channel is designated as Unmoderated, false if not</returns>
        private async Task<bool> ShouldSkip(IGuild guild, IMessageChannel channel)
        {
            var result = false;

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
                Log.LogInformation("Recieved message update event for non-guild message, ignoring");
                return;
            }

            if (await ShouldSkip(guild, channel)) { return; }

            var original = await cachedOriginal.GetOrDownloadAsync();

            //Skip things like embed updates
            if (original.Content == updated.Content) { return; }

            var descriptionText = $"**[Original]({original.GetJumpUrl()})**\n```{FormatMessage(original.Content)}```";

            if (descriptionText.Length <= 2048)
            {
                descriptionText += $"\n**Updated**\n```{FormatMessage(updated.Content)}```";
            }

            var embed = new EmbedBuilder()
                .WithVerboseAuthor(original.Author)
                .WithDescription(descriptionText)
                .WithCurrentTimestamp();

            await TryLog(
                guild,
                $":pencil:Message Edited in {MentionUtils.MentionChannel(channel.Id)}",
                embed.Build());
        }

        private async Task HandleMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var guild = (channel as SocketGuildChannel)?.Guild;

            if (guild == null)
            {
                Log.LogInformation("Recieved message update event for non-guild message, ignoring");
                return;
            }

            if (await ShouldSkip(guild, channel)) { return; }

            var embed = new EmbedBuilder();
            var descriptionContent = $"**Content**\n```Unknown, message not cached```";

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

            await SelfExecuteRequest<IMessageRepository>(async messages => await messages.DeleteAsync(message.Id));

            await TryLog(
                guild,
                $":wastebasket:Message Deleted in {MentionUtils.MentionChannel(channel.Id)} `{message.Id}`",
                embed.Build());
        }

        private async Task HandleMessageReceived(SocketMessage message)
        {
            Log.LogDebug("Handling message received event for message #{MessageId}.", message.Id);

            if (!message.Content.StartsWith('!') &&
                message.Channel is IGuildChannel channel &&
                message.Author is IGuildUser author &&
                author.Guild is SocketGuild guild &&
                !author.IsBot && !author.IsWebhook)
            {
                await SelfExecuteRequest<IMessageRepository>(
                    async messages =>
                    {
                        Log.LogInformation("Logging message #{MessageId} to the database.", message.Id);

                        var entity = new MessageEntity
                        {
                            Id = message.Id,
                            GuildId = guild.Id,
                            ChannelId = channel.Id,
                            AuthorId = author.Id,
                            Timestamp = message.Timestamp
                        };

                        Log.LogDebug("Entity for message #{MessageId}: {@Message}", message.Id, entity);

                        try
                        {
                            await messages.CreateAsync(entity);
                        }
                        catch (Exception ex)
                        {
                            Log.LogError(ex, "An unexpected error occurred when attempting to log message #{MessageId}.", message.Id);
                        }
                    });
            }
        }
    }
}
