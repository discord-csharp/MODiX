using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a summary view of a <see cref="PromotionCampaignEntity"/>, for use in higher layers of the application,
    /// which includes all comment data for the campaign.
    /// </summary>
    public class PromotionCampaignDetails
    {
        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCampaignEntity.Subject"/>.
        /// </summary>
        public GuildUserBrief Subject { get; set; }  = null!;

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
        /// See <see cref="PromotionCampaignEntity.Comments"/>.
        /// </summary>
        public IReadOnlyCollection<PromotionCommentCampaignBrief> Comments { get; set; } = null!;

        [ExpansionExpression]
        internal static Expression<Func<PromotionCampaignEntity, PromotionCampaignDetails>> FromEntityProjection
            = entity => new PromotionCampaignDetails()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Subject = entity.Subject.Project(GuildUserBrief.FromEntityProjection),
                TargetRole = entity.TargetRole.Project(GuildRoleBrief.FromEntityProjection),
                Outcome = entity.Outcome,
                CreateAction = entity.CreateAction.Project(PromotionActionBrief.FromEntityProjection),
                CloseAction = (entity.CloseAction == null)
                    ? null
                    : entity.CloseAction.Project(PromotionActionBrief.FromEntityProjection),
                Comments = entity.Comments.AsQueryable()
                    .Select(PromotionCommentCampaignBrief.FromEntityProjection)
                    .ToArray()
            };
    }
}
