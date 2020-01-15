using Discord;

namespace Modix.Data.Models.Core
{
    public class GuildResult
    {
        public GuildResult(IGuild guild)
        {
            IsError = false;
            Guild = guild;
        }

        public GuildResult(string error)
        {
            IsError = true;
            Error = error;
        }

        public IGuild? Guild { get; }

        public string? Error { get; }

        public bool IsError { get; }
    }
}
