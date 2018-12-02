using System.Collections.Generic;
using Discord;

namespace Modix.Data.Models.Starboard
{
    public class Configuration
    {
        /// <summary>
        /// List of channels to listen to for messages that are starred.
        /// </summary>
        public List<IGuildChannel> AcknowledgedGuildChannels { get; set; }

        /// <summary>
        /// Number of stars per message needed to be placed on the starboard
        /// </summary>
        public int StarsRequired { get; set; }
    }
}
