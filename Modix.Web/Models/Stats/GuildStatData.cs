using Modix.Data.Models.Core;
using Modix.Services.GuildStats;

namespace Modix.Web.Models.Stats;

public record GuildStatData(string GuildName, List<GuildRoleCount> GuildRoleCounts, IReadOnlyCollection<PerUserMessageCount> TopUserMessageCounts);
