using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Services.Core;
using Modix.Data.Models.Core;

namespace Modix.Modules
{
    [Group("pingrole"), Name("Marker Role Manager"), Summary("Allows you to add and remove specific marker roles.")]
    public class MarkerRoleModule : ModuleBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDesignatedRoleService _designatedRoleService;

        public MarkerRoleModule(IAuthorizationService authorizationService, IDesignatedRoleService designatedRoleService)
        {
            _authorizationService = authorizationService;
            _designatedRoleService = designatedRoleService;
        }

        [Command("register")]
        public async Task Register(IRole targetRole)
        {
            if(!(Context.User is IGuildUser user))
            {
                return;
            }

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
        public async Task Unregister(IRole targetRole)
        {
            if (!(Context.User is IGuildUser user))
            {
                return;
            }

            if (!user.RoleIds.Any(x => x == targetRole.Id))
            {
                await ReplyAsync("You're not registered to that role.");
                return;
            }

            if (!await _designatedRoleService.RoleHasDesignationAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable))
            {
                await ReplyAsync("Can't unregister from a role that isn't pingable.");
                return;
            }

            await user.RemoveRoleAsync(targetRole);
            await ReplyAsync($"Unregistered {user.Mention} from role {Format.Bold(targetRole.Name)}.");
        }

        [Command("create")]
        public async Task CreateRole(string targetRoleName)
        {
            if (Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, targetRoleName, StringComparison.OrdinalIgnoreCase)) is IRole)
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
        public async Task DeleteRole(string targetRoleName)
        {
            if (!(Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, targetRoleName, StringComparison.OrdinalIgnoreCase)) is IRole targetRole))
            {
                await ReplyAsync($"Couldn't find role {Format.Bold(targetRoleName)}");
                return;
            }

            if (!(await _designatedRoleService.SearchDesignatedRolesAsync(new DesignatedRoleMappingSearchCriteria()
            {
                CreatedById = Context.User.Id,
                GuildId = Context.Guild.Id,
                IsDeleted = false,
                Type = DesignatedRoleType.Pingable
            }))
            .Any(x => x.Role.Id == targetRole.Id))
            {
                await ReplyAsync("Can't delete a role you didn't create, sorry.");
                return;
            }

            await _designatedRoleService.RemoveDesignatedRoleAsync(Context.Guild.Id, targetRole.Id, DesignatedRoleType.Pingable);

            await targetRole.DeleteAsync();
            await ReplyAsync($"Deleted role {Format.Bold(targetRole.Name)}.");
        }
    }
}
