using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a moderation action performed by an authorized staff member.
    /// </summary>
    [Table("ModerationActions")]
    public class ModerationActionEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="ModerationActionEntity"/>.
        /// </summary>
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The snowflake ID, within the Discord API, of the guild to which this moderation action applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// A timestamp indicating when this entity was created, for auditing purposes.
        /// </summary>
        [Required]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The type of this <see cref="ModerationActionEntity"/>.
        /// </summary>
        [Required]
        public ModerationActionType Type { get; set; }

        /// <summary>
        /// The <see cref="GuildUserEntity.UserId"/> value of <see cref="CreatedBy"/>.
        /// </summary>
        [Required]
        public ulong CreatedById { get; set; }

        /// <summary>
        /// The staff member that applied this moderation action
        /// </summary>
        [Required]
        public virtual GuildUserEntity CreatedBy { get; set; } = null!;

        /// <summary>
        /// The <see cref="InfractionEntity.Id"/> value of <see cref="Infraction"/>.
        /// </summary>
        [ForeignKey(nameof(Infraction))]
        public long? InfractionId { get; set; }

        /// <summary>
        /// If applicable, represents the original infraction reason that was updated
        /// </summary>
        public string? OriginalInfractionReason { get; set; }

        /// <summary>
        /// The <see cref="InfractionEntity"/> to which this <see cref="ModerationActionEntity"/> applies.
        /// Null, if an <see cref="InfractionEntity"/> was not involved in this <see cref="ModerationActionEntity"/>.
        /// </summary>
        public virtual InfractionEntity? Infraction { get; set; }

        /// <summary>
        /// The <see cref="DeletedMessageEntity.MessageId"/> value of <see cref="DeletedMessage"/>.
        /// </summary>
        [ForeignKey(nameof(DeletedMessage))]
        public ulong? DeletedMessageId { get; set; }

        /// <summary>
        /// The <see cref="DeletedMessageEntity"/> to which this <see cref="ModerationActionEntity"/> applies.
        /// Null, if a <see cref="DeletedMessageEntity"/> was not involved in this <see cref="ModerationActionEntity"/>.
        /// </summary>
        public virtual DeletedMessageEntity? DeletedMessage { get; set; }

        /// <summary>
        /// The <see cref="DeletedMessageBatch.Id"/> value of the <see cref="DeletedMessageBatch"/>, if any.
        /// </summary>
        [ForeignKey(nameof(DeletedMessageBatch))]
        public long? DeletedMessageBatchId { get; set; }

        /// <summary>
        /// The <see cref="DeletedMessageBatch"/> to which this <see cref="ModerationActionEntity"/> applies.
        /// Null, if a <see cref="DeletedMessageBatch"/> was not involved in this <see cref="ModerationActionEntity"/>.
        /// </summary>
        public virtual DeletedMessageBatchEntity? DeletedMessageBatch { get; set; }
    }

    public class ModerationActionEntityConfigurator
        : IEntityTypeConfiguration<ModerationActionEntity>
    {
        public void Configure(
            EntityTypeBuilder<ModerationActionEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.Type)
                .HasConversion<string>();

            entityTypeBuilder
                .Property(x => x.CreatedById)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.DeletedMessageId)
                .HasConversion<long>();

            entityTypeBuilder
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.CreatedById });
        }
    }
}
