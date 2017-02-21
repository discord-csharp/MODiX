using Discord;
using Discord.Commands;
using Modix.Services.GuildConfig;
using Monk.Data.Repositories;

namespace Modix
{
    public enum Permissions
    {
        Administrator,
        Moderator
    }

    public class PermissionHelper
    {
        public IRole GetRoleByPermission(ICommandContext context, Permissions perms)
        {
            var guildConfig = new GuildConfigRepository().GetOne(g => g.GuildId == context.Guild.Id);

            if (guildConfig == null)
            {
                throw new GuildConfigException("Guild is not configured yet. Please use the config module to set it up!");
            }

            switch (perms)
            {
                case Permissions.Administrator:
                    return context.Guild.GetRole(guildConfig.AdminRoleId);
                case Permissions.Moderator:
                    return context.Guild.GetRole(guildConfig.ModeratorRoleId);
            }
            // If I ever fuck this up, blame obsi :D
            throw new GuildConfigException("Guild is not configured yet. Please use the config module to set it up!");
        }

    }
}
