using Discord;
using System;

namespace Modix.Services.Utilities
{
    public static class SnowflakeUtilities
    {
        public static bool IsValidSnowflake(ulong snowflake)
        {
            // Jan 1, 2015
            var discordEpoch = SnowflakeUtils.FromSnowflake(0);

            // The supposed timestamp
            var snowflakeDateTime = SnowflakeUtils.FromSnowflake(snowflake);

            return snowflakeDateTime > discordEpoch
                && snowflakeDateTime < DateTimeOffset.UtcNow;
        }
    }
}
