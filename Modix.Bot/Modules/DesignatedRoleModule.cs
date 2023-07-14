using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Humanizer;
using Microsoft.Extensions.Options;
using Modix.Bot.Extensions;
using Modix.Bot.Preconditions;
using Modix.Common.Extensions;
using Modix.Data.Models.Core;
using Modix.Services.CommandHelp;
using Modix.Services.Core;

namespace Modix.Modules
{
    [ModuleHelp("Role Designations", "Configures role designations for various bot services.")]
    [Group("role-designations", "Configures role designations for various bot services.")]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public class DesignatedRoleModule : InteractionModuleBase
    {
        private readonly IDesignatedRoleService _designatedRoleService;
        private readonly ModixConfig _config;

        public DesignatedRoleModule(IDesignatedRoleService designatedRoleService, IOptions<ModixConfig> config)
        {
            _designatedRoleService = designatedRoleService;
            _config = config.Value;
        }

        [SlashCommand("list", "Lists all of the roles designated for use by the bot.")]
        [RequireClaims(AuthorizationClaim.DesignatedRoleMappingRead)]
        public async Task ListAsync()
        {
            var roles = await _designatedRoleService.GetDesignatedRolesAsync(Context.Guild.Id);

            // https://mod.gg/config/roles
            var url = new UriBuilder(_config.WebsiteBaseUrl)
            {
                Path = "/config/roles"
            }.RemoveDefaultPort().ToString();

            var builder = new EmbedBuilder()
            {
                Title = "Assigned Role Designations",
                Url = url,
                Color = Color.Gold,
                Timestamp = DateTimeOffset.UtcNow
            };

            foreach(var type in Enum.GetValues<DesignatedRoleType>())
            {
                var designatedRoles = roles
                    .Where(x => x.Type == type)
                    .OrderByDescending(x => x.Role.Position)
                    .ThenBy(x => x.Role.Id)
                    .ToArray();

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = type.Humanize(),
                    Value = (designatedRoles.Length == 0)
                        ? Format.Italics("No roles assigned")
                        : string.Join(Environment.NewLine, designatedRoles.Select(x => MentionUtils.MentionRole(x.Role.Id))),
                    IsInline = false
                });
            }

            await FollowupAsync(embed: builder.Build());
        }

        [SlashCommand("add", "Assigns a designation to the given role.")]
        [RequireClaims(AuthorizationClaim.DesignatedRoleMappingCreate)]
        public async Task AddAsync(
            [Summary(description: "The role to be assigned a designation.")]
                IRole role,
            [Summary(description: "The designation to assign.")]
                DesignatedRoleType designation)
        {
            await _designatedRoleService.AddDesignatedRoleAsync(role.Guild.Id, role.Id, designation);
            await Context.AddConfirmationAsync();
        }

        [SlashCommand("remove", "Removes a designation from the given role.")]
        [RequireClaims(AuthorizationClaim.DesignatedRoleMappingDelete)]
        public async Task RemoveAsync(
            [Summary(description: "The role whose designation is to be unassigned.")]
                IRole role,
            [Summary(description: "The designation to be unassigned.")]
                DesignatedRoleType designation)
        {
            await _designatedRoleService.RemoveDesignatedRoleAsync(role.Guild.Id, role.Id, designation);
            await Context.AddConfirmationAsync();
        }
    }
}
