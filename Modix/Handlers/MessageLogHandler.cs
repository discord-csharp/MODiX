using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Rest;
using Modix.Services.BehaviourConfiguration;
using Serilog;

namespace Modix.Handlers
{
    public class MessageLogHandler
    {
        private readonly IBehaviourConfiguration _botConfiguration;
        private ISocketMessageChannel _logChannel;

        public MessageLogHandler(IBehaviourConfiguration botConfiguration)
        {
            _botConfiguration = botConfiguration;
        }

        private ISocketMessageChannel GetLogChannel(ISocketMessageChannel context)
        {
            if (_logChannel == null && (context as IGuildChannel)?.Guild is SocketGuild guild)
            {
                var found = guild.Channels.SingleOrDefault(x => x.Id == _botConfiguration.MessageLogBehaviour.LoggingChannelId);
                _logChannel = found as ISocketMessageChannel;
            }

            if (_logChannel == null)
            {
                Log.Error("Could not find log channel for edits/deletes, given id {Id}", new { Id = _botConfiguration.MessageLogBehaviour.LoggingChannelId });
            }

            return _logChannel;
        }

        public async Task LogMessageEdit(IMessage original, IMessage updated, ISocketMessageChannel channel)
        {
            //Skip things like embed updates
            if (original.Content == updated.Content) { return; }

            var embed = new EmbedBuilder()
                .WithAuthor(original.Author)
                .WithDescription($"**Original**```{original.Content}```\n**Updated**```{updated.Content}```")
                .WithTimestamp(DateTimeOffset.UtcNow);

            await GetLogChannel(channel).SendMessageAsync($":pencil:Message Edited in {MentionUtils.MentionChannel(channel.Id)}", embed: embed.Build());
        }

        public async Task LogMessageDelete(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            var embed = new EmbedBuilder();
            var dateDiff = DateTimeOffset.UtcNow - SnowflakeUtils.FromSnowflake(message.Id);

            if (!message.HasValue && dateDiff.TotalDays <= _botConfiguration.MessageLogBehaviour.OldMessageAgeLimit)
            {
                embed = embed.WithDescription($"**Content**```Unknown, message not cached```");
            }
            else
            {
                var cached = message.Value;

                embed = embed
                    .WithAuthor($"{cached.Author.Username}#{cached.Author.Discriminator} `{cached.Author.Id}`", cached.Author.GetAvatarUrl())
                    .WithDescription($"**Content**```{cached.Content}```");
            }
            
            embed = embed.WithTimestamp(DateTimeOffset.UtcNow);
            await GetLogChannel(channel).SendMessageAsync($":wastebasket:Message Deleted in {MentionUtils.MentionChannel(channel.Id)} `{message.Id}`", embed: embed.Build());
        }
    }
}
