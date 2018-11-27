using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Promotions
{
    /// <summary>
    /// Describes a Discord message created for a <see cref="PromotionCommentEntity"/>.
    /// </summary>
    [Table("PromotionCommentMessages")]
    public class PromotionCommentMessageEntity
    {
        /// <summary>
        /// The snowflake ID, within the Discord API, of this message.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong MessageId { get; set; }

        /// <summary>
        /// The <see cref="PromotionCommentEntity.Id"/> of the promotion comment that this message references.
        /// </summary>
        [Required]
        public long CommentId { get; set; }

        /// <summary>
        /// The promotion comment that this message references.
        /// </summary>
        [Required]
        public virtual PromotionCommentEntity Comment { get; set; }

        /// <summary>
        /// The snowflake ID, within the Discord API, of the guild to which this message applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The <see cref="GuildChannelEntity.ChannelId"/> value of the channel that contains this message.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Channel))]
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The channel that contains this message.
        /// </summary>
        [Required]
        public virtual GuildChannelEntity Channel { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<PromotionCommentMessageEntity>()
                .Property(x => x.MessageId)
                .HasConversion<long>();

            modelBuilder
                .Entity<PromotionCommentMessageEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<PromotionCommentMessageEntity>()
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            modelBuilder
                .Entity<PromotionCommentMessageEntity>()
                .HasOne(x => x.Comment)
                .WithMany()
                .HasForeignKey(x => x.CommentId);

            modelBuilder
                .Entity<PromotionCommentMessageEntity>()
                .HasOne(x => x.Channel)
                .WithMany()
                .HasForeignKey(x => x.ChannelId);
        }
    }
}
