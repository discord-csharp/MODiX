using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.CommandHelp;

namespace Modix.Modules
{
    [Name("Move Message"), RequireUserPermission(GuildPermission.ManageMessages), HiddenFromHelp]
    public class MoveMessageModule : ModuleBase
    {
        [Command("move"), Summary("Moves a message from one channel to another."), Remarks("Usage: `!move 12345 #foo`")]
        public async Task Run([Remainder] string command)
        {
            if (Context.User.IsBot) return;

            var commandSplit = command.Split(' ');

            if (commandSplit.Length < 2)
            {
                await ReplyAsync($"!move requires at least 2 arguments: The messae ID, and the target channel");
                return;
            }

            (ulong messageID, string channel, string reason) = (ulong.Parse(commandSplit[0]), commandSplit[1], string.Join(' ', commandSplit.Skip(2)));

            // Source message
            var message = await Context.Channel.GetMessageAsync(messageID, CacheMode.AllowDownload);
            if (message == null)
            {
                await ReplyAsync($"`{message}` must be an existing message id. Right click on the message and choose `Copy ID` to grab its id");
                return;
            }

            // Target channel
            var channels = await Context.Guild.GetTextChannelsAsync();
            var targetChannel = channels.SingleOrDefault(c => c.Mention.Equals(channel, StringComparison.OrdinalIgnoreCase));
            if (targetChannel == null)
            {
                await ReplyAsync($"`{channel}` must be an existing channel, and referenced in a mention format: such as `#{channel}`");
                return;
            }

            // Format output
            var builder = new EmbedBuilder()
                    .WithColor(new Color(95, 186, 125))
                    .WithDescription(message.Content)
                    .WithFooter($"Message copied from #{message.Channel.Name} by @{Context.User.Username}");

            // Optional reason
            if (!string.IsNullOrWhiteSpace(reason))
            {
                builder.Footer.Text += $" with reason \"{reason}\"";
            }

            builder.Build();

            await targetChannel.SendMessageAsync("", embed: builder);

            // Delete the source message, and the command message that started this request
            await message.DeleteAsync();
            await Context.Message.DeleteAsync();
        }
    }
}
