﻿using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a summary view of a <see cref="PromotionActionEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class PromotionActionSummary
    {
        /// <summary>
        /// See <see cref="PromotionActionEntity.Id"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.Created"/>.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.Type"/>.
        /// </summary>
        public PromotionActionType Type { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.CreatedBy"/>.
        /// </summary>
        public GuildUserBrief CreatedBy { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.Campaign"/>.
        /// </summary>
        public PromotionCampaignBrief Campaign { get; set; }

        /// <summary>
        /// See <see cref="PromotionActionEntity.Comment"/>.
        /// </summary>
        public PromotionCommentActionBrief Comment { get; set; }
        
        [ExpansionExpression]
        internal static Expression<Func<PromotionActionEntity, PromotionActionSummary>> FromEntityProjection
            = entity => new PromotionActionSummary()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                Created = entity.Created,
                Type = entity.Type,
                CreatedBy = entity.CreatedBy.Project(GuildUserBrief.FromEntityProjection),
                Campaign = (entity.Campaign == null)
                    ? null
                    : entity.Campaign.Project(PromotionCampaignBrief.FromEntityProjection),
                Comment = (entity.Comment == null)
                    ? null
                    : entity.Comment.Project(PromotionCommentActionBrief.FromEntityProjection)
            };
    }
}
