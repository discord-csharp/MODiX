using System.Collections.Generic;
using Discord;
using Modix.Data.Models.Core;

namespace Modix.Services.Promotions
{
    public class CreateCampaignContinuationData
    {
        public CreateCampaignContinuationData(
            IGuildUser subject, GuildRoleBrief targetRankRole, IEnumerable<GuildRoleBrief> rankRoles, string comment, ulong nominatingUserId)
            => (Subject, TargetRankRole, RankRoles, Comment, NominatingUserId) = (subject, targetRankRole, rankRoles, comment, nominatingUserId);

        public string Comment { get; }

        public ulong NominatingUserId { get; }

        public IEnumerable<GuildRoleBrief> RankRoles { get; }

        public IGuildUser Subject { get; }

        public GuildRoleBrief TargetRankRole { get; }

        public void Deconstruct(
            out IGuildUser subject, out GuildRoleBrief targetRankRole, out IEnumerable<GuildRoleBrief> rankRoles, out string comment, out ulong nominatingUserId)
            => (subject, targetRankRole, rankRoles, comment, nominatingUserId) = (Subject, TargetRankRole, RankRoles, Comment, NominatingUserId);
    }
}
