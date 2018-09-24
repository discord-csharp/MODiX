using System;
using System.Collections.Generic;
using System.Text;
using Modix.Data.Models.Core;

namespace Modix.WebServer.Models
{
    public class DesignatedChannelApiData
    {
        public long Id { get; set; }
        public ulong ChannelId { get; set; }
        public DesignatedChannelType ChannelDesignation { get; set; }
        public string Name { get; set; }
    }
}
