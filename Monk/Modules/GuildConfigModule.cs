using Discord;
using Discord.Commands;
using Monk.Services.GuildConfig;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Monk.Modules
{
    [Group("config"), Name("Config"), Summary("Configures MODFiX for use on your server")]
    public class GuildConfigModule : ModuleBase
    {
        private GuildConfigService service = new GuildConfigService();

        [Command("SetAdmin"), Summary("Allows you to set the role ID of the Administrators")]
        public async Task SetAdminAsync(ulong roleId)
        {
            if (!await isOwnerAsync()) return;

            await service.SetPermissionAsync(Context.Guild, Permissions.Administrator, roleId);
            await ReplyAsync($"Permission for Administrators has been successfully updated to {roleId}");
        }

        [Command("SetModerator"), Summary("Allows you to set the role ID of the Moderators")]
        public async Task SetModeratorAsync(ulong roleId)
        {
            if (!await isOwnerAsync()) return;

            await service.SetPermissionAsync(Context.Guild, Permissions.Moderator, roleId);
            await ReplyAsync($"Permission for Moderators has been successfully updated to {roleId}");
        }

        [Command("show"), Summary("Shows current config.")]
        public async Task ShowConfigAsync()
        {
            var res = await service.GenerateFormattedConfig(Context.Guild);
            await ReplyAsync(res);
        }

        [Command("GetRoles"), Summary("Shows a list of all roles including their Ids that are on this guild.")]
        public async Task GetRolesAsync()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var role in Context.Guild.Roles)
            {
                // Replacing to avoid those nasty pings.
                sb.Append($"{role.Name} - {role.Id}\n".Replace("@everyone", "everyone"));
            }
            await ReplyAsync(sb.ToString());
        }

        private async Task<bool> isOwnerAsync()
        {
            var owner = await Context.Guild.GetOwnerAsync();
            if (owner.Id != Context.User.Id)
            {
                await ReplyAsync("Configuration is restricted to the GuildOwner");
                return false;
            }
            return true;
        }
    }
}
