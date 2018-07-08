using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Modix.Modules
{
    [Name("Channel Viewer"), Summary("Allows you to show and hide specific channels.")]
    public class ChannelViewerModule : ModuleBase
    {
        public async Task HideChannelAsync(IGuildChannel channel)
        {
            await channel.AddPermissionOverwriteAsync(Context.User, OverwritePermissions.DenyAll(channel));
            await ReplyAsync("Channel hidden.");
        }

        public async Task ShowChannelAsync(IGuildChannel channel)
        {
            await channel.AddPermissionOverwriteAsync(Context.User, OverwritePermissions.AllowAll(channel));
            await ReplyAsync("Channel hidden.");
        }
    }
}