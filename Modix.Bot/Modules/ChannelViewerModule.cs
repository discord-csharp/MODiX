using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Modix.Modules
{
    [Group("channel"), Name("Channel Viewer"), Summary("Allows you to show and hide specific channels.")]
    public class ChannelViewerModule : ModuleBase
    {
        [Command("hide")]
        public async Task HideChannelAsync(IGuildChannel channel)
        {
            await channel.AddPermissionOverwriteAsync(Context.User,
                new OverwritePermissions(readMessages: PermValue.Deny, sendMessages: PermValue.Deny));
            await ReplyAsync("Channel hidden.");
        }

        [Command("show")]
        public async Task ShowChannelAsync(IGuildChannel channel)
        {
            await channel.AddPermissionOverwriteAsync(Context.User,
                new OverwritePermissions(readMessages: PermValue.Allow, sendMessages: PermValue.Allow));
            await ReplyAsync("Channel hidden.");
        }
    }
}