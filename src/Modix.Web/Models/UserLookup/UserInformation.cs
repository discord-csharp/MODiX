using Discord;
using Discord.WebSocket;
using Modix.Data.Models.Core;
using Modix.Web.Models.Common;

namespace Modix.Web.Models.UserLookup;

public record UserInformation(
    string Id,
    string? Username,
    string? Nickname,
    string? Discriminator,
    string? AvatarUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? JoinedAt,
    DateTimeOffset? FirstSeen,
    DateTimeOffset? LastSeen,
    int Rank,
    int Last7DaysMessages,
    int Last30DaysMessages,
    decimal AverageMessagesPerDay,
    int Percentile,
    IEnumerable<RoleInformation> Roles,
    bool IsBanned,
    string? BanReason,
    bool IsGuildMember,
    IReadOnlyList<MessageCountPerChannelInformation> MessageCountsPerChannel
)
{
    public static UserInformation FromEphemeralUser(
        EphemeralUser ephemeralUser,
        GuildUserParticipationStatistics userRank,
        IReadOnlyList<MessageCountByDate> messages7,
        IReadOnlyList<MessageCountByDate> messages30,
        SocketRole[] roles,
        List<MessageCountPerChannelInformation> messageCountsPerChannel)
    {
        return new UserInformation(
            ephemeralUser.Id.ToString(),
            ephemeralUser.Username,
            ephemeralUser.Nickname,
            ephemeralUser.Discriminator,
            ephemeralUser.AvatarId != null ? ephemeralUser.GetAvatarUrl(ImageFormat.Auto, 256) : ephemeralUser.GetDefaultAvatarUrl(),
            ephemeralUser.CreatedAt,
            ephemeralUser.JoinedAt,
            ephemeralUser.FirstSeen,
            ephemeralUser.LastSeen,
            userRank.Rank,
            messages7.Sum(x => x.MessageCount),
            messages30.Sum(x => x.MessageCount),
            userRank.AveragePerDay,
            userRank.Percentile,
            roles
                .Where(x => !x.IsEveryone)
                .Select(x => new RoleInformation(x.Id, x.Name, x.Color.ToString())),
            ephemeralUser.IsBanned,
            ephemeralUser.BanReason,
            ephemeralUser.GuildId != default,
            messageCountsPerChannel
        );
    }
}
