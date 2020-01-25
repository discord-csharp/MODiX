using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Humanizer;
using Modix.Services.Utilities;

namespace Modix.Services.UserInfo
{
    public class UserInfoEmbedBuilderHelper
    {
        private readonly StringBuilder _content = new StringBuilder();
        private readonly DateTimeOffset _nowUtc = DateTimeOffset.UtcNow;

        public static UserInfoEmbedBuilderHelper Create()
        {
            return new UserInfoEmbedBuilderHelper();
        }

        private UserInfoEmbedBuilderHelper()
        {
            _content.AppendLine("**\u276F User Information**");
        }

        public UserInfoEmbedBuilderHelper WithId(ulong userId)
        {
            _content.AppendLine("ID: " + userId);
            return this;
        }

        public UserInfoEmbedBuilderHelper WithClickableMention(ulong userId)
        {
            _content.AppendLine("Profile: " + MentionUtils.MentionUser(userId));
            return this;
        }

        public UserInfoEmbedBuilderHelper WithFirstAndLastSeen(DateTimeOffset firstSeen, DateTimeOffset lastSeen)
        {
            _content.AppendLine($"First Seen: {FormatUtilities.FormatTimeAgo(_nowUtc, firstSeen)}");
            _content.AppendLine($"Last Seen: {FormatUtilities.FormatTimeAgo(_nowUtc, lastSeen)}");

            return this;
        }

        public UserInfoEmbedBuilderHelper WithStatus(UserStatus status)
        {
            _content.AppendLine($"Status: {status.Humanize()}");
            return this;
        }

        public UserInfoEmbedBuilderHelper WithBan(string reason)
        {
            _content.AppendLine($"🔨\\ **Banned**: {reason}");
            return this;
        }

        public UserInfoEmbedBuilderHelper WithParticipationInPeriod(int rank,
            int numberOfMessagesIn30Days,
            int numberOfMessagesIn7Days,
            double averagePerDay,
            double percentile)
        {
            _content.AppendLine("\n**\u276F Guild Participation**");
            _content.AppendLine($"Rank: {rank} {GetParticipationEmoji(rank)}");
            _content.AppendLine($"Last 7 days: {numberOfMessagesIn7Days}");
            _content.AppendLine($"Last 30 days: {numberOfMessagesIn30Days}");
            _content.AppendLine($"Average/day: {averagePerDay} (top {percentile})");

            return this;
        }

        public UserInfoEmbedBuilderHelper WithChannel(ulong channelId,
            int numberOfMessagesInChannel)
        {
            _content.AppendLine($"Most active channel: {MentionUtils.MentionChannel(channelId)} ({numberOfMessagesInChannel} messages)");
            return this;
        }

        public UserInfoEmbedBuilderHelper WithMemberInformation(IGuildUser user)
        {
            _content.AppendLine("\n**\u276F Member Information**");

            if (!string.IsNullOrEmpty(user.Nickname))
            {
                _content.AppendLine("Nickname: " + user.Nickname);
            }

            _content.AppendLine($"Created: {FormatUtilities.FormatTimeAgo(_nowUtc, user.CreatedAt)}");

            if (user.JoinedAt != null)
            {
                _content.AppendLine($"Joined: {FormatUtilities.FormatTimeAgo(_nowUtc, user.JoinedAt.Value)}");
            }

            if (user.RoleIds?.Count > 0)
            {
                var roles = user.RoleIds.Select(x => user.Guild.Roles.Single(y => y.Id == x))
                    .Where(x => x.Id != x.Guild.Id)
                    .OrderByDescending(role => role)
                    .ToArray();

                if (roles.Any())
                {
                    _content.Append($"{"Role".ToQuantity(roles.Length, ShowQuantityAs.None)}: ");
                    _content.AppendLine(roles.Select(r => r.Mention).Humanize());
                }
            }

            return this;
        }

        public UserInfoEmbedBuilderHelper WithPromotions(List<(ulong targetRoleId, DateTimeOffset date)> promotions)
        {
            _content.AppendLine(Format.Bold("\n**\u276F Promotion History**"));

            foreach (var promotion in promotions.OrderByDescending(x => x.date))
            {
                _content.AppendLine($"• {MentionUtils.MentionRole(promotion.targetRoleId)} {FormatUtilities.FormatTimeAgo(_nowUtc, promotion.date)}");
            }

            return this;
        }

        public string Build()
        {
            return _content.ToString();
        }

        private string GetParticipationEmoji(int rank) =>
            rank switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => "🏆",
            };
    }
}
