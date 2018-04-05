using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Data.Utilities;
using System;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data;

namespace Modix.Services.Utilities
{
    public class RequireModixPermissionAttribute : PreconditionAttribute
    {
        private readonly Permissions _requiredPermission;

        public RequireModixPermissionAttribute(Permissions requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            // first check if we're in a guild because this won't work if we aren't in a guild
            if (!(context.User is IGuildUser))
                return PreconditionResult.FromError("The current context isn't a guild");

            var db = map.GetService<PermissionHelper>();
            
            var user = (IGuildUser)context.User;
            var role = await db.GetRoleByPermission(context, _requiredPermission);

            if (user.RoleIds.Any(roleId => user.Guild.GetRole(roleId).Position > role.Position))
            {
                return PreconditionResult.FromSuccess();
            }
            
            return PreconditionResult.FromError("The current user doesn't satisfy a permission precondition");
        }
    }
}
