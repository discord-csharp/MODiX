using System;
using System.Linq.Expressions;

using Modix.Data.ExpandableQueries;
using Modix.Data.Models.Core;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a summary view of a <see cref="PromotionCommentMessageEntity"/>, for use in higher layers of the application.
    /// </summary>
    public class PromotionCommentMessageSummary
    {
        /// <summary>
        /// See <see cref="PromotionCommentMessageEntity.MessageId"/>.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentMessageEntity.Comment"/>.
        /// </summary>
        public PromotionCommentMessageBrief Comment { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentMessageEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentMessageEntity.Channel"/>.
        /// </summary>
        public GuildChannelBrief Channel { get; set; }

        [ExpansionExpression]
        internal static Expression<Func<PromotionCommentMessageEntity, PromotionCommentMessageSummary>> FromEntityProjection
            = entity => new PromotionCommentMessageSummary
            {
                MessageId = entity.MessageId,
                Comment = (entity.Comment == null)
                    ? null
                    : entity.Comment.Project(PromotionCommentMessageBrief.FromEntityProjection),
                GuildId = entity.GuildId,
                Channel = (entity.Channel == null)
                    ? null
                    : entity.Channel.Project(GuildChannelBrief.FromEntityProjection),
            };
    }
}
