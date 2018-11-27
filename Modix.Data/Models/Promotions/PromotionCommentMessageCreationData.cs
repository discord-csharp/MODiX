namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes an operation to create a <see cref="PromotionCommentMessageEntity"/>.
    /// </summary>
    public class PromotionCommentMessageCreationData
    {
        /// <summary>
        /// See <see cref="PromotionCommentMessageEntity.MessageId"/>.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentMessageEntity.CommentId"/>.
        /// </summary>
        public long CommentId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentMessageEntity.GuildId"/>.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// See <see cref="PromotionCommentMessageEntity.ChannelId"/>.
        /// </summary>
        public ulong ChannelId { get; set; }

        public PromotionCommentMessageEntity ToEntity()
            => new PromotionCommentMessageEntity
            {
                MessageId = MessageId,
                CommentId = CommentId,
                GuildId = GuildId,
                ChannelId = ChannelId,
            };
    }
}
