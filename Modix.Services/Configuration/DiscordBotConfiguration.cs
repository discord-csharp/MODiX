using System.Collections.Generic;

namespace Modix.Services.Configuration
{
    public class DiscordBotConfiguration
    {
        public bool PurgeInvites => InvitePurging != null;

        public InvitePurgingConfiguration InvitePurging { get; set; }

        public class InvitePurgingConfiguration
        {
            public List<ulong> ExemptRoles { get; set; }
            public ulong LoggingChannelId { get; set; }
        }
    }
}