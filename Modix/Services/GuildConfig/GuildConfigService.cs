using System.Linq;
using System.Threading.Tasks;
using Discord;
using Modix.Data;
using Modix.Data.Models;
using Modix.Data.Repositories;
using Modix.Utilities;

namespace Modix.Services.GuildConfig
{
    public sealed class GuildConfigService
    {
        private DiscordGuildRepository _guildRepository = new DiscordGuildRepository();
        private DiscordGuild _guild = null;

        private GuildConfigService() { }

        public GuildConfigService(IGuild guild)
        {
            _guild = _guildRepository.GetByGuildAsync(guild).Result ?? _guildRepository.AddByGuildAsync(guild).Result;
        }

        public Task<bool> IsPermittedAsync(IGuild guild, IGuildUser user, Permissions reqPerm)
        {
            if (_guild.Config == null)
            {
                throw new GuildConfigException("Guild is not configured yet. Please use the config module to set it up!");
            }

            ulong requiredRoleId = 0;

            switch(reqPerm)
            {
                case Permissions.Administrator:
                    requiredRoleId = _guild.Config.AdminRoleId.ToUlong();
                    break;
                case Permissions.Moderator:
                    requiredRoleId = _guild.Config.ModeratorRoleId.ToUlong();
                    break;
            }

            return Task.Run(() => user.RoleIds.Any(x => x == guild.GetRole(requiredRoleId).Id));
        }

        public async Task SetPermissionAsync(IGuild guild, Permissions permission, ulong roleId)
        {
            using (var db = new ModixContext())
            {
                if (_guild.Config == null)
                {
                    _guild.Config = new Data.Models.GuildConfig
                    {
                        GuildId = guild.Id.ToLong(),
                        AdminRoleId = permission == Permissions.Administrator ? roleId.ToLong() : 0,
                        ModeratorRoleId = permission == Permissions.Moderator ? roleId.ToLong() : 0,
                    };

                    db.Guilds.Update(_guild);
                    return;
                }

                if (permission == Permissions.Administrator)
                {
                    _guild.Config.AdminRoleId = roleId.ToLong();
                }
                else
                {
                    _guild.Config.ModeratorRoleId = roleId.ToLong();
                }

                db.Guilds.Update(_guild);
            }
        }

        public async Task<string> GenerateFormattedConfig(IGuild guild)
        {
            if (_guild.Config == null)
            {
                return "This guild has no configuration at the moment. You can create a configuration by setting up roles through !config.";
            }

            return $"AdminRole: {_guild.Config.AdminRoleId}\nModerationRole: {_guild.Config.ModeratorRoleId}";
        }
    }
}
