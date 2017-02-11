using Discord;
using Monk.Data.Repositorys;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Monk.Services.GuildConfig
{
    public class GuildConfigService
    {
        private GuildConfigRepository repository = new GuildConfigRepository();

        public Task<bool> IsPermittedAsync(IGuild guild, IGuildUser user, Permissions reqPerm)
        {
            var guildConfig = repository.GetOne(x => x.GuildId == guild.Id);
            if (guildConfig == null)
            {
                throw new GuildConfigException("Guild is not configured yet. Please use the config module to set it up!");
            }

            ulong requiredRoleId = 0;

            switch(reqPerm)
            {
                case Permissions.Administrator:
                    requiredRoleId = guildConfig.AdminRoleId;
                    break;
                case Permissions.Moderator:
                    requiredRoleId = guildConfig.ModeratorRoleId;
                    break;
            }

            return Task.Run(() => user.RoleIds.Where(x => x == guild.GetRole(requiredRoleId).Id).Count() > 0);
        }

        public async Task SetPermissionAsync(IGuild guild, Permissions permission, ulong roleId)
        {
            var guildConfig = repository.GetOne(x => x.GuildId == guild.Id);

            if(guildConfig == null)
            {
                guildConfig = new Monk.Data.Models.GuildConfig
                {
                    GuildId = guild.Id,
                    AdminRoleId = permission == Permissions.Administrator ? roleId : 0,
                    ModeratorRoleId = permission == Permissions.Moderator ? roleId : 0,
                };

                await repository.InsertAsync(guildConfig);
                return;
            }

            if(permission == Permissions.Administrator)
            {
                guildConfig.AdminRoleId = roleId;
            }
            else
            {
                guildConfig.ModeratorRoleId = roleId;
            }

            var res = await repository.Update(guildConfig.Id, guildConfig);
        }
    }
}
