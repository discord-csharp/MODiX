using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Humanizer;

using Modix.Data.Models.Core;
using Modix.Services.CommandHelp;
using Modix.Services.Core;

namespace Modix.Modules
{
    [Group("pingrole")]
    [Alias("pingroles", "pr")]
    [Name("Topic Roles")]
    [Summary("Provides functionality for maintaining and registering or unregistering topic roles.")]
    [HelpTags("marker", "pingroles", "pingable", "topicroles")]
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
                    Type = DesignatedRoleType.Pingable,
                    IsDeleted = false,
                });

            var pingRolesInformation = pingRoles
                .OrderBy(x => x.Role.Name)
                .Select(x =>
                {
                    return Context.Guild.GetRole(x.Role.Id) is ISocketRole role
                        ? $"{role.Mention} - {Format.Bold("member".ToQuantity(role.Members.Count()))}"
                        : null;
                })
                .Where(x => x != null)
                .ToArray();

            var pingableRolesFormatted = "Pingable role".ToQuantity(pingRolesInformation.Length, ShowQuantityAs.None);
            var pingRolesInformationFormatted = pingRolesInformation.Length == 0
                                                    ? "No Pingable Roles available."
                                                    : string.Join("\n", pingRolesInformation);


            var embed = new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithTitle($"{pingableRolesFormatted} ({pingRolesInformation.Length})")
                .WithDescription(pingRolesInformationFormatted)
                .WithFooter("Register to any of the above with !pingrole register <RoleName>");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("register")]
        [Alias("join")]
        [RequireContext(ContextType.Guild)]
        [Summary("Registers the user as a member of the supplied pingrole.")]
        public async Task RegisterAsync(
            [Remainder]
            [Summary("The role to register to.")]
                IRole targetRole)
        {
            var user = Context.User as IGuildUser;

            if (user.RoleIds.Any(x => x == targetRole.Id))
            {
                await ReplyAsync("You're already registered to that role.");
                return;
            }

            if (!await _designatedRoleService.RoleHasDesignationAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable, default))
            {
                await ReplyAsync("Can't register to a role that isn't pingable.");
                return;
            }

            await user.AddRoleAsync(targetRole);
            await ReplyAsync($"Registered {user.Mention} to {Format.Bold(targetRole.Name)}.");
        }

        [Command("unregister")]
        [Alias("leave")]
        [RequireContext(ContextType.Guild)]
        [Summary("Unregisters the user from being a member of the supplied pingrole.")]
        public async Task UnregisterAsync(
            [Remainder]
            [Summary("The role to unregister from.")]
                IRole targetRole)
        {
            var user = Context.User as IGuildUser;

            if (!user.RoleIds.Any(x => x == targetRole.Id))
            {
                await ReplyAsync("You're not registered to that role.");
                return;
            }

            if (!await _designatedRoleService
                .RoleHasDesignationAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable, default))
            {
                await ReplyAsync("Can't unregister from a role that isn't pingable.");
                return;
            }

            await user.RemoveRoleAsync(targetRole);
            await ReplyAsync($"Unregistered {user.Mention} from role {Format.Bold(targetRole.Name)}.");
        }

        [Command("create")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Creates a new pingable role.")]
        public async Task CreateRoleAsync(
            [Remainder]
            [Summary("The name of the new pingable role.")]
                string targetRoleName)
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

            var targetRole = await Context.Guild.CreateRoleAsync(targetRoleName, isMentionable: true);

            await _designatedRoleService.AddDesignatedRoleAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable);

            await ReplyAsync($"Created pingable role {Format.Bold(targetRole.Name)}.");
        }

        [Command("delete")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Deletes an existing pingable role.")]
        public async Task DeleteRoleAsync(
            [Remainder]
            [Summary("The pingable role to delete.")]
                IRole role)
        {
            await _designatedRoleService.RemoveDesignatedRoleAsync(Context.Guild.Id, role.Id, DesignatedRoleType.Pingable);

            await role.DeleteAsync();
            await ReplyAsync($"Deleted role {Format.Bold(role.Name)}.");
        }
    }
}
