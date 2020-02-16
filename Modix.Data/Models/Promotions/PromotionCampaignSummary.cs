using System;
using System.Linq;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a summary view of a <see cref="PromotionCampaignEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class PromotionCampaignSummary
    {
        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.GuildId"/>.
        // </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Subject"/>.
        /// </summary>
        public GuildUserBrief Subject { get; set; } = null!;

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.TargetRole"/>.
        /// </summary>
        public GuildRoleBrief TargetRole { get; set; } = null!;

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.CreateAction"/>.
        /// </summary>
        public PromotionActionBrief CreateAction { get; set; } = null!;

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Outcome"/>.
        /// </summary>
        public PromotionCampaignOutcome? Outcome { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.CloseAction"/>.
        /// </summary>
        public PromotionActionBrief? CloseAction { get; set; }

        /// <summary>
        /// The number of <see cref="PromotionCampaignEntity.Comments" /> records whose
        /// <see cref="PromotionCommentEntity.Sentiment" /> value is <see cref="PromotionSentiment.Abstain" />.
        /// </summary>
        public int AbstainCount { get; set; }

        /// <summary>
        /// The number of <see cref="PromotionCampaignEntity.Comments" /> records whose
        /// <see cref="PromotionCommentEntity.Sentiment" /> value is <see cref="PromotionSentiment.Approve" />.
        /// </summary>
        public int ApproveCount { get; set; }

        /// <summary>
        /// The number of <see cref="PromotionCampaignEntity.Comments" /> records whose
        /// <see cref="PromotionCommentEntity.Sentiment" /> value is <see cref="PromotionSentiment.Oppose" />.
        /// </summary>
        public int OpposeCount { get; set; }

        [ExpansionExpression]
        internal static Expression<Func<PromotionCampaignEntity, PromotionCampaignSummary>> FromEntityProjection
            = entity => new PromotionCampaignSummary()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Subject = entity.Subject.Project(GuildUserBrief.FromEntityProjection),
                TargetRole = entity.TargetRole.Project(GuildRoleBrief.FromEntityProjection),
                CreateAction = entity.CreateAction.Project(PromotionActionBrief.FromEntityProjection),
                Outcome = entity.Outcome,
                CloseAction = (entity.CloseAction == null)
                    ? null
                    : entity.CloseAction.Project(PromotionActionBrief.FromEntityProjection),
                // TODO: Retrieve these Count values with a .GroupBy(x => x.Sentiment).ToDictionary(x.Key, x => x.Count()) subquery if EF Core ever is able to support it.
                // Right now there are a variety of issues tracking problems with .GroupBy() subqueries. E.G.
                // https://github.com/dotnet/efcore/issues/18836
                // https://github.com/dotnet/efcore/issues/15097
                // https://github.com/dotnet/efcore/issues/10012
                AbstainCount = entity.Comments
                    .Where(y => (y.ModifyActionId == null)
                        && (y.Sentiment == PromotionSentiment.Abstain))
                    .Count(),
                ApproveCount = entity.Comments
                    .Where(y => (y.ModifyActionId == null)
                        && (y.Sentiment == PromotionSentiment.Approve))
                    .Count(),
                OpposeCount = entity.Comments
                    .Where(y => (y.ModifyActionId == null)
                        && (y.Sentiment == PromotionSentiment.Oppose))
                    .Count(),
            };
    }
}
