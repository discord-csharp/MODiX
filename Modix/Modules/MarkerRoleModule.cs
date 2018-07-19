using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace Modix.Modules
{
    [Group("pingrole"), Name("Marker Role Manager"), Summary("Allows you to add and remove specific marker roles.")]
    public class MarkerRoleModule : ModuleBase
    {
        private List<ulong> _roleIds;

        public MarkerRoleModule(List<ulong> roleIds)
        {
            _roleIds = roleIds;
        }

        [Command("add")]
        public async Task AddRole([Remainder] IRole targetRole) 
        {
            if (!_roleIds.Contains(targetRole.Id))
            {
                await ReplyAsync("Cannot add that role.");
                return;
            }

            var guildUser = (IGuildUser) Context.User;
            await guildUser.AddRoleAsync(targetRole);
            await ReplyAsync($"Added role `{targetRole.Name}`.");
        }

        [Command("remove")]
        public async Task RemoveRole([Remainder] IRole targetRole)
        {
            if (!_roleIds.Contains(targetRole.Id))
            {
                await ReplyAsync("Cannot remove that role.");
                return;
            }

            var guildUser = (IGuildUser) Context.User;
            await guildUser.RemoveRoleAsync(targetRole);
            await ReplyAsync($"Removed role `{targetRole.Name}`.");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("register")]
        public async Task RegisterRole([Remainder] IRole targetRole)
        {
            if (_roleIds.Contains(targetRole.Id))
            {
                await ReplyAsync("Cannot register that role - it has already been registered.");
                return;
            }

            _roleIds.Add(targetRole.Id);
            await ReplyAsync($"Registered role `{targetRole.Name}`.");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("unregister")]
        public async Task UnregisterRole([Remainder] IRole targetRole)
        {
            if (!_roleIds.Contains(targetRole.Id))
            {
                await ReplyAsync("Cannot register that role that role - it is not registered.");
                return;
            }

            _roleIds.Remove(targetRole.Id);
            await ReplyAsync($"Unregistered role `{targetRole.Name}`.");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("create")]
        public async Task CreateRole([Remainder] string targetRoleName)
        {
            if (Context.Guild.Roles.Any(x => string.Equals(x.Name, targetRoleName, StringComparison.OrdinalIgnoreCase)))
            {
                await ReplyAsync("Cannot create that role - it has already been registered.");
                return;
            }

            var targetRole = await Context.Guild.CreateRoleAsync(targetRoleName);
            await targetRole.ModifyAsync(x => x.Mentionable = true);
            _roleIds.Add(targetRole.Id);
            await ReplyAsync($"Created and registered role `{targetRole.Name}`.");
        }

        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("delete")]
        public async Task DeleteRole([Remainder] IRole targetRole)
        {
            if (!Context.Guild.Roles.Contains(targetRole))
            {
                await ReplyAsync("Cannot delete that role - It does not exist. Quite frankly Discord.NET should have barfed before this so if you're seeing this something broke.");
                return;
            }

            if (!_roleIds.Any(x => Context.Guild.Roles.Any(y => y.Id == x)))
            {
                await ReplyAsync("Cannot delete that role - it is not a marker role.");
                return;
            }

            _roleIds.Remove(targetRole.Id);
            await targetRole.DeleteAsync();
            await ReplyAsync($"Deleted and unregistered role `{targetRole.Name}`.");
        }
    }
}