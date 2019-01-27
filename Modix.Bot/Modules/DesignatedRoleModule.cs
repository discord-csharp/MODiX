using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Modules
{
    [Name("Role Designations")]
    [Summary("Configures role designations for various bot services")]
    [Group("role designations")]
    public class DesignatedRoleModule : ModuleBase
    {
        public IAuthorizationService AuthorizationService { get; }
        public IDesignatedRoleService DesignatedRoleService { get; }

        public DesignatedRoleModule(IAuthorizationService authorizationService, IDesignatedRoleService designatedRoleService)
        {
            AuthorizationService = authorizationService;
            DesignatedRoleService = designatedRoleService;
        }

        [Command]
        [Summary("Lists all of the roles designated for use by the bot")]
        public async Task List()
        {
            var roles = await DesignatedRoleService.GetDesignatedRolesAsync(Context.Guild.Id);

            var builder = new EmbedBuilder()
            {
                Title = "Assigned Role Designations",
                Url = "https://mod.gg/config/roles",
                Color = Color.Gold,
                Timestamp = DateTimeOffset.UtcNow
            };

            foreach(var type in Enum.GetValues(typeof(DesignatedRoleType)).Cast<DesignatedRoleType>())
            {
                var designatedRoles = roles
                    .Where(x => x.Type == type)
                    .OrderByDescending(x => x.Role.Position)
                    .ThenBy(x => x.Role.Id)
                    .ToArray();

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = Format.Bold(type.Humanize()),
                    Value = (designatedRoles.Length == 0)
                        ? Format.Italics("No roles assigned")
                        : designatedRoles
                            .Select(x => MentionUtils.MentionRole(x.Role.Id))
                            .Aggregate(string.Empty, (x, y) => $"{x}\n{y}"),
                    IsInline = false
                });
            }

            await ReplyAsync(string.Empty, false, builder.Build());
        }

        [Command("add")]
        [Summary("Assigns a designation to the given role")]
        public Task Add(
            [Summary("The role to be assigned a designation")]
                IRole role,
            [Summary("The designation to assign")]
                DesignatedRoleType designation)
            => DesignatedRoleService.AddDesignatedRoleAsync(role.Guild.Id, role.Id, designation);

        [Command("remove")]
        [Summary("Removes a designation from the given role")]
        public Task RemoveLogRole(
            [Summary("The role whose designation is to be unassigned")]
                IRole role,
            [Summary("The designation to be unassigned")]
                DesignatedRoleType designation)
            => DesignatedRoleService.RemoveDesignatedRoleAsync(role.Guild.Id, role.Id, designation);
    }
}
