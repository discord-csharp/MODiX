using System.Collections.Generic;

namespace Modix.Services.BehaviourConfiguration
{
    public class BehaviourConfigurationDto
    {
        public InvitePurgeBehaviour InvitePurgeBehaviour { get; set; }
    }

    public class InvitePurgeBehaviour
    {
        public bool IsEnabled { get; set; }
        public IEnumerable<ulong> ExemptRoleIds { get; set; }
        public ulong LoggingChannelId { get; set; }
    }
}