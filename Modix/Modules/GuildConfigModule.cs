using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Modix.Services.GuildConfig;
using Monk;

namespace Modix.Modules
{
    [Group("config"), Name("Config"), Summary("Configures MODFiX for use on your server")]
    public class GuildConfigModule : ModuleBase
    {
        private GuildConfigService service = new GuildConfigService();

        [Command("SetAdmin"), Summary("Allows you to set the role ID of the Administrators"), RequireOwner]
        public async Task SetAdminAsync(ulong roleId)
        {
            await service.SetPermissionAsync(Context.Guild, Permissions.Administrator, roleId);
            await ReplyAsync($"Permission for Administrators has been successfully updated to {roleId}");
        }

        [Command("SetModerator"), Summary("Allows you to set the role ID of the Moderators"), RequireOwner]
        public async Task SetModeratorAsync(ulong roleId)
        {
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

            foreach (var role in Context.Guild.Roles.OrderBy(r => r.Position))
            {
                // Replacing to avoid those nasty pings.
                sb.Append($"{role.Name} - {role.Id}\n".Replace("@everyone", "everyone"));
            }
            await ReplyAsync(sb.ToString());
        }
    }
}
