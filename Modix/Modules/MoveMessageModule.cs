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
        [Command("move")]
        public async Task Run([Remainder] string command)
        {
            if (Context.User.IsBot) return;

            var commandSplit = command.Split(' ');
            (ulong messageID, string channel, string reason) = (ulong.Parse(commandSplit[0]), commandSplit[1], commandSplit.ElementAtOrDefault(2));
            var message = await Context.Channel.GetMessageAsync(messageID, CacheMode.AllowDownload);
            var channels = await Context.Guild.GetTextChannelsAsync();
            var targetChannel = channels.SingleOrDefault(c => c.Mention.Equals(channel, StringComparison.OrdinalIgnoreCase));

            var builder = new EmbedBuilder()
                    .WithColor(new Color(95, 186, 125))
                    .WithDescription(message.Content)
                    .WithFooter($"Message copied from #{message.Channel.Name} by @{Context.User.Username}");

            builder.Build();

            await targetChannel.SendMessageAsync("", embed: builder);
            await message.DeleteAsync();
            await Context.Message.DeleteAsync();
        }
    }
}
