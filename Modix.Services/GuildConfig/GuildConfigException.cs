using System;

namespace Modix.Services.GuildConfig
{
    public class GuildConfigException : Exception
    {
        public GuildConfigException()
        {
        }

        public GuildConfigException(string message) : base(message)
        {
        }

        public GuildConfigException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}