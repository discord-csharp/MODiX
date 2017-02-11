using Discord;
using Monk.Data.Repositorys;
using Monk.Services.GuildConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monk.Utilitys
{
    public static class IGuildUserExtensions
    {
        public static async Task<bool> IsAllowedAsync(this IGuildUser user, Permissions reqPerm)
        {
            var guildConfig = new GuildConfigRepository().GetOne(x => x.GuildId == user.GuildId);
            if (guildConfig == null)
            {
                throw new GuildConfigException("Guild is not configured yet. Please use the config module to set it up!");
            }
            ulong requiredRoleId = 0;

            switch (reqPerm)
            {
                case Permissions.Administrator:
                    requiredRoleId = guildConfig.AdminRoleId;
                    break;
                case Permissions.Moderator:
                    requiredRoleId = guildConfig.ModeratorRoleId;
                    break;
            }

            return user.RoleIds.Contains(user.Guild.GetRole(requiredRoleId).Id);
        }
    }
}
