using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Services.Utilities;

namespace Modix.Services.Core
{
    public class MessageLogBehavior : BehaviorBase
    {
        private readonly DiscordSocketClient _discordClient;

        public MessageLogBehavior(DiscordSocketClient discordClient, IServiceProvider serviceProvider, ILogger<MessageLogBehavior> logger) : base(serviceProvider)
        {
            _discordClient = discordClient;

            Log = logger ?? NullLogger<MessageLogBehavior>.Instance;
        }

        private ILogger<MessageLogBehavior> Log { get; }

        protected internal override Task OnStartingAsync()
        {
            _discordClient.MessageDeleted += HandleMessageDelete;
            _discordClient.MessageUpdated += HandleMessageEdit;
            _discordClient.MessageReceived += HandleMessageReceived;

            return Task.CompletedTask;
        }

        protected internal override Task OnStoppedAsync()
        {
            _discordClient.MessageDeleted -= HandleMessageDelete;
            _discordClient.MessageUpdated -= HandleMessageEdit;
            _discordClient.MessageReceived -= HandleMessageReceived;

            return Task.CompletedTask;
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
                result = await designatedChannelService.ChannelHasDesignation(guild, channel, ChannelDesignation.Unmoderated);
            });

            return result;
        }

        private async Task HandleMessageReceived(SocketMessage message)
        {
            Log.LogDebug("Handling message received event for message #{MessageId}.", message.Id);

            if (!message.Content.StartsWith('!') &&
                message.Channel is IGuildChannel channel &&
                channel.Guild is IGuild guild &&
                message.Author is IGuildUser author &&
                !author.IsBot && !author.IsWebhook)
            {
                await SelfExecuteRequest<ModixContext>(async db =>
                {
                    Log.LogInformation("Logging message #{MessageId} to the database.", message.Id);

                    var entity = new MessageEntity
                    {
                        MessageId = message.Id,
                        GuildId = guild.Id,
                        ChannelId = channel.Id,
                        UserId = author.Id,
                        Timestamp = message.Timestamp,
                        OriginalMessageHash = ComputeHash(message.Content)
                    };

                    Log.LogDebug("Entity for message #{MessageId}: {@Message}", message.Id, entity);

                    try
                    {
                        await db.Messages.AddAsync(entity);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ex, "An unexpected error occurred when attempting to log message #{MessageId}.", message.Id);
                    }
                });
            }
        }

        private async Task HandleMessageEdit(Cacheable<IMessage, ulong> cachedOriginal, SocketMessage updated, ISocketMessageChannel channel)
        {
            //Don't log when Modix edits its own messages
            if (updated.Author.Id == _discordClient.CurrentUser.Id) { return; }

            var guild = (channel as SocketGuildChannel)?.Guild;

            if (guild == null)
            {
                Log.LogInformation("Recieved message update event for non-guild message, ignoring");
            }

            if (await ShouldSkip(guild, channel)) { return; }

            var original = await cachedOriginal.GetOrDownloadAsync();

            //Skip things like embed updates
            if (original.Content == updated.Content) { return; }
            
            var embed = new EmbedBuilder()
                .WithVerboseAuthor(original.Author)
                .WithDescription($"**Original**```{MessageIfEmpty(original.Content)}```\n **Updated**```{MessageIfEmpty(updated.Content)}```")
                .WithVerboseTimestamp(DateTimeOffset.UtcNow);

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
                Log.LogInformation("Recieved message update event for non-guild message, ignoring");
            }

            if (await ShouldSkip(guild, channel)) { return; }

            var embed = new EmbedBuilder();

            if (!message.HasValue)
            {
                embed = embed.WithDescription($"**Content**```Unknown, message not cached```");
            }
            else
            {
                var cached = message.Value;

                //Don't log when messages from Modix are deleted
                if (message.Value.Author.Id == _discordClient.CurrentUser.Id) { return; }

                embed = embed
                    .WithVerboseAuthor(cached.Author)
                    .WithDescription($"**Content**```{MessageIfEmpty(cached.Content)}```");

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

            embed = embed.WithVerboseTimestamp(DateTimeOffset.UtcNow);

            await SelfExecuteRequest<IDesignatedChannelService>(async designatedChannelService =>
            {
                await designatedChannelService.SendToDesignatedChannelsAsync(
                    guild, ChannelDesignation.MessageLog,
                    $":wastebasket:Message Deleted in {MentionUtils.MentionChannel(channel.Id)} `{message.Id}`", embed.Build());
            });
        }

        private static string MessageIfEmpty(string input, string ifEmpty = "Empty Message Content")
        {
            return string.IsNullOrWhiteSpace(input) ? "Empty Message Content" : input;
        }

        private static string ComputeHash(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var alg = SHA1.Create())
            {
                var hash = alg.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
