using Discord;
using Discord.Commands;
using Modix.Data;
using Modix.Data.Repositories;
using Modix.Services.GuildConfig;

namespace Modix.Utilities
{
    public enum Permissions
    {
        Administrator,
        Moderator
    }

    public class PermissionHelper
    {
        private static readonly DiscordGuildRepository GuildRepository = new DiscordGuildRepository();
        public IRole GetRoleByPermission(ICommandContext context, Permissions perms)
        {
            using (var db = new ModixContext())
            {
                var guild = GuildRepository.GetByGuildAsync(context.Guild).Result ??
                            GuildRepository.AddByGuildAsync(context.Guild).Result;
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
}
