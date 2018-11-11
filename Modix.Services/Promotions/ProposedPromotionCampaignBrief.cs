using System.Collections.Generic;
using Discord;
using Modix.Data.Models.Core;

namespace Modix.Services.Promotions
{
    public class ProposedPromotionCampaignBrief
    {
        public string Comment { get; set; }

        public ulong NominatingUserId { get; set; }

        public IEnumerable<GuildRoleBrief> RankRoles { get; set; }

        public IGuildUser Subject { get; set; }

        public GuildRoleBrief TargetRankRole { get; set; }

        public void Deconstruct(
            out IGuildUser subject, out GuildRoleBrief targetRankRole, out IEnumerable<GuildRoleBrief> rankRoles, out string comment, out ulong nominatingUserId)
            => (subject, targetRankRole, rankRoles, comment, nominatingUserId) = (Subject, TargetRankRole, RankRoles, Comment, NominatingUserId);
    }
}
