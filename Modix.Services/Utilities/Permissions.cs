using Discord;
using Discord.Commands;
using Modix.Data;
using Modix.Data.Services;
using Modix.Data.Utilities;
using Modix.Services.GuildConfig;

namespace Modix.Services.Utilities
{
    public class PermissionHelper
    {
        private readonly DiscordGuildService GuildService;
        public PermissionHelper(ModixContext context)
        {
            GuildService = new DiscordGuildService(context);
        }

        public IRole GetRoleByPermission(ICommandContext context, Permissions perms)
        {
            var guild = GuildService.ObtainAsync(context.Guild).Result;
            if (guild.Config == null)
            {
                throw new GuildConfigException(
                    "DiscordGuild is not configured yet. Please use the config module to set it up!");
            }

            switch (perms)
            {
                case Permissions.Administrator:
                    return context.Guild.GetRole(guild.Config.AdminRoleId.ToUlong());
                case Permissions.Moderator:
                    return context.Guild.GetRole(guild.Config.ModeratorRoleId.ToUlong());
            }
            // If I ever fuck this up, blame obsi :D
            throw new GuildConfigException(
                "DiscordGuild is not configured yet. Please use the config module to set it up!");
        }
    }
}
