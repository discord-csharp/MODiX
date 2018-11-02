using System.Collections.Generic;
using Modix.Data.Models.Core;

namespace Modix.Models
{
    public class DesignatedChannelCreationData
    {
        public ulong ChannelId { get; set; }
        public List<DesignatedChannelType> ChannelDesignations { get; set; }
    }
}
