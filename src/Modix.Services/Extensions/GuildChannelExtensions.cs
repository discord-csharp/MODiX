using Discord;

namespace Modix.Services.Extensions
{
    public static class GuildChannelExtensions
    {
        public static bool IsPublic(this IGuildChannel channel)
        {
            if (channel?.Guild is IGuild guild)
            {
                var permissions = channel.GetPermissionOverwrite(guild.EveryoneRole);

                return !permissions.HasValue || permissions.Value.ViewChannel != PermValue.Deny;
            }

            return false;
        }
    }
}
