using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Options;
using Modix.Bot.Extensions;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Modules
{
    [Name("Role Designations")]
    [Summary("Configures role designations for various bot services.")]
    [Group("role designations")]
    public class DesignatedRoleModule : ModuleBase
    {
        public IAuthorizationService AuthorizationService { get; }
        public IDesignatedRoleService DesignatedRoleService { get; }

        public ModixConfig Config { get; }

        public DesignatedRoleModule(IAuthorizationService authorizationService, IDesignatedRoleService designatedRoleService, IOptions<ModixConfig> config)
        {
            AuthorizationService = authorizationService;
            DesignatedRoleService = designatedRoleService;
            Config = config.Value;
        }

        [Command]
        [Summary("Lists all of the roles designated for use by the bot")]
        public async Task ListAsync()
        {
            var roles = await DesignatedRoleService.GetDesignatedRolesAsync(Context.Guild.Id);

            // https://mod.gg/config/roles
            var url = new UriBuilder(Config.WebsiteBaseUrl)
            {
                Path = "/config/roles"
            }.ToString();

            var builder = new EmbedBuilder()
            {
                Title = "Assigned Role Designations",
                Url = url,
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
                    Name = type.Humanize(),
                    Value = (designatedRoles.Length == 0)
                        ? Format.Italics("No roles assigned")
                        : string.Join(Environment.NewLine, designatedRoles.Select(x => MentionUtils.MentionRole(x.Role.Id))),
                    IsInline = false
                });
            }

            await ReplyAsync(embed: builder.Build());
        }

        [Command("add")]
        [Summary("Assigns a designation to the given role")]
        public async Task AddAsync(
            [Summary("The role to be assigned a designation")]
                IRole role,
            [Summary("The designation to assign")]
                DesignatedRoleType designation)
        {
            await DesignatedRoleService.AddDesignatedRoleAsync(role.Guild.Id, role.Id, designation);
            await Context.AddConfirmation();
        }

        [Command("remove")]
        [Summary("Removes a designation from the given role")]
        public async Task RemoveAsync(
            [Summary("The role whose designation is to be unassigned")]
                IRole role,
            [Summary("The designation to be unassigned")]
                DesignatedRoleType designation)
        {
            await DesignatedRoleService.RemoveDesignatedRoleAsync(role.Guild.Id, role.Id, designation);
            await Context.AddConfirmation();
        }
    }
}
