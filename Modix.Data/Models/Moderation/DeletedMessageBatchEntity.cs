using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a collection of messages that were deleted simultaneously.
    /// </summary>
    [Table("DeletedMessageBatches")]
    public class DeletedMessageBatchEntity
    {
        /// <summary>
        /// The integer that uniquely identifies this batch.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> of the action that created this batch.
        /// </summary>
        [Required]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The action that created this batch.
        /// </summary>
        [Required]
        public virtual ModerationActionEntity CreateAction { get; set; } = null!;

        /// <summary>
        /// The messages that were deleted simultaneously to form this batch.
        /// </summary>
        [Required]
        public virtual IReadOnlyCollection<DeletedMessageEntity> DeletedMessages { get; set; } = null!;
    }

    public class DeletedMessageBatchEntityConfigurator
        : IEntityTypeConfiguration<DeletedMessageBatchEntity>
    {
        public void Configure(
            EntityTypeBuilder<DeletedMessageBatchEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<DeletedMessageBatchEntity>(x => x.CreateActionId);
        }
    }
}
