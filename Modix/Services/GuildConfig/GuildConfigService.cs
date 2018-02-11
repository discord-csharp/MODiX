using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Modix.Data;
using Modix.Data.Models;
using Modix.Data.Services;
using Modix.Data.Utilities;
using Modix.Utilities;

namespace Modix.Services.GuildConfig
{
    public sealed class GuildConfigService
    {
        private DiscordGuildService _guildService;
        private DiscordGuild _guild = null;

        public GuildConfigService(IGuild guild, ModixContext context)
        {
            _guildService = new DiscordGuildService(context);
            _guild = _guildService.ObtainAsync(guild).Result;
        }

        public Task<bool> IsPermittedAsync(IGuild guild, IGuildUser user, Permissions reqPerm)
        {
            if (_guild.Config == null)
            {
                throw new GuildConfigException("Guild is not configured yet. Please use the config module to set it up!");
            }

            ulong requiredRoleId = 0;

            switch (reqPerm)
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

        public void SetPermissionAsync(IGuild guild, Permissions permission, ulong roleId)
        {
            _guildService.SetPermissionAsync(guild, permission, roleId);
        }

        public string GenerateFormattedConfig(IGuild guild)
        {
            if (_guild.Config == null)
            {
                return "This guild has no configuration at the moment. You can create a configuration by setting up roles through !config.";
            }

            return $"AdminRole: {_guild.Config.AdminRoleId}\nModerationRole: {_guild.Config.ModeratorRoleId}";
        }

        public async Task<bool> AddModuleLimitAsync(IMessageChannel channel, string module)
        {
            return await _guildService.AddModuleLimitAsync(_guild, channel, module);
        }

        public async Task<bool> RemoveModuleLimitAsync(IMessageChannel channel, string module)
        {
            return await _guildService.RemoveModuleLimitAsync(_guild, channel, module);
        }
    }
}
