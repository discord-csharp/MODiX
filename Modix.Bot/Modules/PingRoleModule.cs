using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Humanizer;

using Modix.Data.Models.Core;
using Modix.Services.CommandHelp;
using Modix.Services.Core;

namespace Modix.Modules
{
    [ModuleHelp("Ping Roles", "Provides functionality for maintaining and registering or unregistering ping roles.")]
    [Group("pingrole", "Provides functionality for maintaining and registering or unregistering ping roles.")]
    [HelpTags("marker", "pingroles", "pingable", "topicroles")]
    public class PingRoleModule : InteractionModuleBase
    {
        private readonly IDesignatedRoleService _designatedRoleService;

        public PingRoleModule(IDesignatedRoleService designatedRoleService)
        {
            _designatedRoleService = designatedRoleService;
        }

        [SlashCommand("list", "List all available ping roles.")]
        public async Task ListAsync()
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
                    return Context.Guild.GetRole(x.Role.Id) is SocketRole role
                        ? $"{role.Mention} - {Format.Bold("member".ToQuantity(role.Members.Count()))}"
                        : null;
                })
                .Where(x => x is not null)
                .ToArray();

            var pingableRolesFormatted = "Ping role".ToQuantity(pingRolesInformation.Length, ShowQuantityAs.None);
            var pingRolesInformationFormatted = pingRolesInformation.Length == 0
                                                    ? "No ping roles available."
                                                    : string.Join("\n", pingRolesInformation);

            var embed = new EmbedBuilder()
                .WithAuthor(Context.Guild.Name, Context.Guild.IconUrl)
                .WithColor(Color.Blue)
                .WithTitle($"{pingableRolesFormatted} ({pingRolesInformation.Length})")
                .WithDescription(pingRolesInformationFormatted)
                .WithFooter("Register to any of the above with /pingrole register <RoleName>");

            await FollowupAsync(embed: embed.Build());
        }

        [SlashCommand("register", "Registers the user as a member of the supplied ping role.")]
        public async Task RegisterAsync(
            [Summary(description: "The role to register to.")]
                IRole targetRole)
        {
            var user = Context.User as IGuildUser;

            if (user.RoleIds.Any(x => x == targetRole.Id))
            {
                await FollowupAsync("You're already registered to that role.");
                return;
            }

            if (!await _designatedRoleService.RoleHasDesignationAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable, default))
            {
                await FollowupAsync("Can't register to a role that isn't designated as a ping role.");
                return;
            }

            await user.AddRoleAsync(targetRole);
            await FollowupAsync($"Registered {user.Mention} to {targetRole.Mention}.", allowedMentions: AllowedMentions.None);
        }

        [SlashCommand("unregister", "Unregisters the user from being a member of the supplied ping role.")]
        public async Task UnregisterAsync(
            [Summary(description: "The role to unregister from.")]
                IRole targetRole)
        {
            var user = Context.User as IGuildUser;

            if (!user.RoleIds.Any(x => x == targetRole.Id))
            {
                await FollowupAsync("You're not registered to that role.");
                return;
            }

            if (!await _designatedRoleService
                .RoleHasDesignationAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable, default))
            {
                await FollowupAsync("Can't unregister from a role that isn't designated as a ping role.");
                return;
            }

            await user.RemoveRoleAsync(targetRole);
            await FollowupAsync($"Unregistered {user.Mention} from role {targetRole.Mention}.", allowedMentions: AllowedMentions.None);
        }

        [SlashCommand("create", "Creates a new ping role.")]
        [DefaultMemberPermissions(GuildPermission.ManageRoles)]
        public async Task CreateRoleAsync(
            [Summary(description: "The name of the new ping role.")]
                string targetRoleName)
        {
            if (Context.Guild.Roles
                .Any(x => string.Equals(
                    x.Name,
                    targetRoleName,
                    StringComparison.OrdinalIgnoreCase)))
            {
                await FollowupAsync("Cannot create that role - it already exists, did you mean to register to it?");
                return;
            }

            var targetRole = await Context.Guild.CreateRoleAsync(targetRoleName, isMentionable: true);

            await _designatedRoleService.AddDesignatedRoleAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable);

            await FollowupAsync($"Created ping role {targetRole.Mention}.", allowedMentions: AllowedMentions.None);
        }

        [SlashCommand("delete", "Deletes an existing ping role.")]
        [DefaultMemberPermissions(GuildPermission.ManageRoles)]
        public async Task DeleteRoleAsync(
            [Summary(description: "The ping role to delete.")]
                IRole role)
        {
            await _designatedRoleService.RemoveDesignatedRoleAsync(Context.Guild.Id, role.Id, DesignatedRoleType.Pingable);

            await role.DeleteAsync();
            await FollowupAsync($"Deleted role {Format.Bold(role.Name)}.", allowedMentions: AllowedMentions.None);
        }
    }
}
