using Discord;
using Discord.Commands;
using Monk.Data.Repositories;
using Monk.Services.GuildConfig;
using System.Linq;
using System.Threading.Tasks;

namespace Monk
{
    public class RequireModixPermissionAttribute : PreconditionAttribute
    {
        private Permissions _permission;

        public RequireModixPermissionAttribute(Permissions permission)
        {
            _permission = permission;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            // first check if we're in a guild because this won't work if we aren't in a guild
            if (!(context.User is IGuildUser user))
                return Task.Run(() => PreconditionResult.FromError("The current context isn't a guild"));

            var guildConfig = new GuildConfigRepository().GetOne(g => g.GuildId == context.Guild.Id) ?? throw new GuildConfigException("Guild is not configured yet. Please use the config module to set it up!");
            
            switch (_permission)
            {
                case Permissions.Moderator:
                    return Task.Run(() => user.RoleIds.Contains(guildConfig.ModeratorRoleId) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("The current user doesn't satisfy a permission precondition"));
                case Permissions.Administrator:
                    return Task.Run(() => user.RoleIds.Contains(guildConfig.AdminRoleId) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("The current user doesn't satisfy a permission precondition"));
                default:
                    return Task.Run(() => PreconditionResult.FromError("Undefined operation, tell a programmer!"));
            }
        }
    }
}
