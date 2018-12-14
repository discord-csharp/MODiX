using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modix.Data.Models.Core;
using Modix.Services.GuildStats;

namespace Modix.Models
{
    public class GuildStatApiData
    {
        public string GuildName { get; set; }
        public List<GuildRoleCount> GuildRoleCounts { get; set; }
        public IReadOnlyDictionary<string, int> TopUserMessageCounts { get; set; }
    }
}
