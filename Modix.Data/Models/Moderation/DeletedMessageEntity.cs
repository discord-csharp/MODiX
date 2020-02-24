using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a message that was automatically deleted by the application.
    /// </summary>
    [Table("DeletedMessages")]
    public class DeletedMessageEntity
    {
        /// <summary>
        /// The snowflake ID, within the Discord API, of the message that was deleted.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong MessageId { get; set; }

        /// <summary>
        /// The snowflake ID, within the Discord API, of the guild to which this infraction applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The <see cref="GuildChannelEntity.ChannelId"/> value of <see cref="Channel"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Channel))]
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The channel from which the message was deleted.
        /// </summary>
        [Required]
        public virtual GuildChannelEntity Channel { get; set; } = null!;

        /// <summary>
        /// The <see cref="GuildUserEntity.UserId"/> value of <see cref="Author"/>.
        /// </summary>
        [Required]
        public ulong AuthorId { get; set; }

        /// <summary>
        /// The user that authored the deleted message.
        /// </summary>
        [Required]
        public virtual GuildUserEntity Author { get; set; } = null!;

        /// <summary>
        /// The content of the deleted message.
        /// </summary>
        [Required]
        public string Content { get; set; } = null!;

        /// <summary>
        /// A description of the reason that the message was deleted.
        /// </summary>
        [Required]
        public string Reason { get; set; } = null!;

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        public long? CreateActionId { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> that created this <see cref="DeletedMessageEntity"/>.
        /// </summary>
        public virtual ModerationActionEntity CreateAction { get; set; } = null!;

        /// <summary>
        /// The <see cref="DeletedMessageBatchEntity.Id"/> value of the batch
        /// that this <see cref="DeletedMessageEntity"/> belongs to.
        /// </summary>
        [ForeignKey(nameof(Batch))]
        public long? BatchId { get; set; }

        /// <summary>
        /// The batch that this <see cref="DeletedMessageEntity"/> belongs to.
        /// </summary>
        public virtual DeletedMessageBatchEntity? Batch { get; set; }
    }

    public class DeletedMessageEntityConfigurator
        : IEntityTypeConfiguration<DeletedMessageEntity>
    {
        public void Configure(
            EntityTypeBuilder<DeletedMessageEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.MessageId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.AuthorId)
                .HasConversion<long>();

            entityTypeBuilder
                .HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.AuthorId });

            entityTypeBuilder
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<DeletedMessageEntity>(x => x.CreateActionId);
        }
    }
}
