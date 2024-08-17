namespace Modix.Web.Shared.Models.Stats;

public record GuildStatData(string GuildName, IReadOnlyCollection<GuildRoleMemberCount> GuildRoleCounts, IReadOnlyCollection<PerUserMessageCount> TopUserMessageCounts);
