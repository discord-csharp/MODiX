using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.Core;
using Modix.Data.Models.Core;
using Humanizer;
using Discord.WebSocket;

namespace Modix.Modules
{
    [Group("pingrole")]
    [Alias("pingroles")]
    [Name("Marker Role Manager")]
    [Summary("Allows you to add and remove specific marker roles.")]
    public class MarkerRoleModule : ModuleBase
    {
        private readonly IDesignatedRoleService _designatedRoleService;

        public MarkerRoleModule(IDesignatedRoleService designatedRoleService)
        {
            _designatedRoleService = designatedRoleService;
        }

        [Command("list")]
        [Alias("")]
        public async Task List()
        {
            var pingRoles = await _designatedRoleService
                .SearchDesignatedRolesAsync(new DesignatedRoleMappingSearchCriteria()
                {
                    Type = DesignatedRoleType.Pingable
                });

            var pingRolesInformation = pingRoles.Select(x =>
            {
                var role = Context.Guild.GetRole(x.Role.Id) as ISocketRole;
                return $"{role.Mention} - {Format.Bold("member".ToQuantity(role.Members.Count()))}";
            })
            .ToArray();

            var pingableRolesFormatted = "Pingable role".ToQuantity(pingRolesInformation.Length, ShowQuantityAs.None);

            var embed = new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithTitle($"{pingableRolesFormatted} ({pingRolesInformation.Length})")
                .WithDescription(string.Join("\n", pingRolesInformation))
                .WithFooter("Register to any of the above with !pingrole register <RoleName>");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("register")]
        [RequireContext(ContextType.Guild)]
        public async Task Register(IRole targetRole)
        {
            var user = Context.User as IGuildUser;

            if (user.RoleIds.Any(x => x == targetRole.Id))
            {
                await ReplyAsync("You're already registered to that role.");
                return;
            }

            if (!await _designatedRoleService.RoleHasDesignationAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable))
            {
                await ReplyAsync("Can't register to a role that isn't pingable.");
                return;
            }

            await user.AddRoleAsync(targetRole);
            await ReplyAsync($"Registered {user.Mention} to {Format.Bold(targetRole.Name)}.");
        }

        [Command("unregister")]
        [RequireContext(ContextType.Guild)]
        public async Task Unregister(IRole targetRole)
        {
            var user = Context.User as IGuildUser;

            if (!user.RoleIds.Any(x => x == targetRole.Id))
            {
                await ReplyAsync("You're not registered to that role.");
                return;
            }

            if (!await _designatedRoleService
                .RoleHasDesignationAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable))
            {
                await ReplyAsync("Can't unregister from a role that isn't pingable.");
                return;
            }

            await user.RemoveRoleAsync(targetRole);
            await ReplyAsync($"Unregistered {user.Mention} from role {Format.Bold(targetRole.Name)}.");
        }

        [Command("create")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task CreateRole([Remainder] string targetRoleName)
        {
            if (Context.Guild.Roles
                .Any(x => string.Equals(
                    x.Name,
                    targetRoleName,
                    StringComparison.OrdinalIgnoreCase)))
            {
                await ReplyAsync("Cannot create that role - it already exists, did you mean to register to it?");
                return;
            }

            var targetRole = await Context.Guild.CreateRoleAsync(targetRoleName);
            await targetRole.ModifyAsync(f => f.Mentionable = true);

            await _designatedRoleService.AddDesignatedRoleAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable);

            await ReplyAsync($"Created pingable role {Format.Bold(targetRole.Name)}.");
        }

        [Command("delete")]
        public async Task DeleteRole(IRole role)
        {
            await _designatedRoleService.RemoveDesignatedRoleAsync(Context.Guild.Id, role.Id, DesignatedRoleType.Pingable);

            await role.DeleteAsync();
            await ReplyAsync($"Deleted role {Format.Bold(role.Name)}.");
        }
    }
}
