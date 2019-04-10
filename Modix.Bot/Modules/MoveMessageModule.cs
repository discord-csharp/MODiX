using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Services.CommandHelp;
using Modix.Services.Utilities;
using Serilog;

namespace Modix.Modules
{
    [Name("Move Message"), RequireUserPermission(GuildPermission.ManageMessages), HiddenFromHelp]
    public class MoveMessageModule : ModuleBase
    {
        [Command("move"), Summary("Moves a message from one channel to another."), Remarks("Usage: !move 12345 #foo")]
        public async Task Run(ulong messageId, SocketTextChannel channel, [Remainder] string reason = null)
        {
            // Ignore bots and same channel-to-channel requests
            if (Context.User.IsBot || channel.Id == Context.Channel.Id) return;

            IMessage message = null;

            try
            {
                message = await channel.GetMessageAsync(messageId);

                if (message == null)
                    message = await FindMessageInUnknownChannel(messageId);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed fetching message for Move command, ran by {User} with a Message ID of {MessageId}",
                    Context.User.Mention, messageId);
            }

            // Format output
            var builder = new EmbedBuilder()
                    .WithColor(new Color(95, 186, 125))
                    .WithDescription(message.Content);

            if (!string.IsNullOrWhiteSpace(reason))
            {
                builder.AddField("Reason", reason, true);
            }

            var mover = $"{Context.User.GetFullUsername()}";
            builder.WithFooter($"Message copied from #{message.Channel.Name} by {mover}");

            await channel.SendMessageAsync("", embed: builder.Build());

            // Delete the source message, and the command message that started this request
            await message.DeleteAsync();
            await Context.Message.DeleteAsync();
            await Context.Channel.SendMessageAsync($"Message {messageId} moved by {mover}");
        }

        [Command("move"), Summary("Moves a message from one channel to another."), Remarks("Usage: !move 12345 #foo")]
        public async Task Run(SocketTextChannel channel, ulong messageId, [Remainder] string reason = null)
            => await Run(messageId, channel, reason);

        private async Task<IMessage> FindMessageInUnknownChannel(ulong messageId)
        {
            IMessage message = null;

            // We haven't found a message, now fetch all text
            // channels and attempt to find the message

            var channels = await Context.Guild.GetTextChannelsAsync();

            foreach (var channel in channels)
            {
                try
                {
                    message = await channel.GetMessageAsync(messageId);

                    if (message != null)
                        break;
                }
                catch (Exception e)
                {
                    Log.Debug(e, "Failed accessing channel {ChannelName} when searching for message {MessageId}",
                        channel.Name, messageId);
                }
            }

            return message;
        }
    }
}
