using System;

namespace Monk.Services.GuildConfig
{
    public class GuildConfigException : Exception
    {
        public GuildConfigException() : base() { }
        public GuildConfigException(string message) : base(message) { }
        public GuildConfigException(string message, Exception innerException) : base(message, innerException) { }
    }
}
