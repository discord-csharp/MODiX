using System.Collections.Generic;
using Modix.Data.Models.Core;
using Modix.Services.GuildStats;

namespace Modix.Models
{
    public class GuildStatApiData
    {
        public string GuildName { get; set; }

        public List<GuildRoleCount> GuildRoleCounts { get; set; }

        public IReadOnlyCollection<PerUserMessageCount> TopUserMessageCounts { get; set; }
    }
}
