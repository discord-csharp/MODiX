using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Services.BehaviourConfiguration
{
    public class MessageLogBehaviour
    {
        public ulong LoggingChannelId { get; set; }

        /// <summary>
        /// How old (in days) can uncached messages be for them to be logged when deleted?
        /// </summary>
        public uint OldMessageAgeLimit { get; set; }
    }
}
