using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes a user-configurable, reproduceable message.
    /// </summary>
    [Table("Tags")]
    public class TagEntity
    {
        /// <summary>
        /// The tag's unique identifier.
        /// </summary>
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which the tag belongs.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The unique identifier of the action that created the tag.
        /// </summary>
        [Required]
        [ForeignKey(nameof(CreateAction))]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The action that created the tag.
        /// </summary>
        public virtual TagActionEntity CreateAction { get; set; } = null!;

        /// <summary>
        /// The unique identifier of the action that deleted the tag.
        /// </summary>
        [ForeignKey(nameof(DeleteAction))]
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The action that deleted the tag.
        /// </summary>
        public virtual TagActionEntity? DeleteAction { get; set; }

        /// <summary>
        /// The unique string that is used to invoke the tag.
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The message that will be displayed when the tag is invoked.
        /// </summary>
        [Required]
        public string Content { get; set; } = null!;

        /// <summary>
        /// The number of times that the tag has been invoked.
        /// </summary>
        [Required]
        public uint Uses { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the user to whom the tag belongs, if any.
        /// </summary>
        public ulong? OwnerUserId { get; set; }

        /// <summary>
        /// The user to whom the tag belongs, if any.
        /// </summary>
        public virtual GuildUserEntity? OwnerUser { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the role to which the tag belongs, if any.
        /// </summary>
        public ulong? OwnerRoleId { get; set; }

        /// <summary>
        /// The role to which the tag belongs, if any.
        /// </summary>
        public virtual GuildRoleEntity? OwnerRole { get; set; }

        public void IncrementUse()
        {
            Uses++;
        }

        public void Update(string newContent)
        {
            Content = newContent;
        }

        public void Delete(ulong deletedByUserId)
        {
            var deleteAction = new TagActionEntity
            {
                GuildId = GuildId,
                Created = DateTimeOffset.UtcNow,
                Type = TagActionType.TagDeleted,
                CreatedById = deletedByUserId,
                OldTagId = Id,
            };

            DeleteAction = deleteAction;
        }

        public void TransferToUser(ulong userId)
        {
            OwnerUserId = userId;
            OwnerRoleId = null;
        }

        public void TransferToRole(ulong roleId)
        {
            OwnerUserId = null;
            OwnerRoleId = roleId;
        }
    }

    public class TagEntityConfiguration
        : IEntityTypeConfiguration<TagEntity>
    {
        public void Configure(
            EntityTypeBuilder<TagEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.OwnerUserId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.OwnerRoleId)
                .HasConversion<long>();

            entityTypeBuilder
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<TagEntity>(x => x.CreateActionId);

            entityTypeBuilder
                .HasOne(x => x.DeleteAction)
                .WithOne()
                .HasForeignKey<TagEntity>(x => x.DeleteActionId);

            entityTypeBuilder
                .HasOne(x => x.OwnerUser)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.OwnerUserId });

            entityTypeBuilder
                .HasOne(x => x.OwnerRole)
                .WithMany()
                .HasForeignKey(x => x.OwnerRoleId);

            entityTypeBuilder
                .HasIndex(x => x.GuildId);

            entityTypeBuilder
                .HasIndex(x => x.Name);

            entityTypeBuilder
                .HasIndex(x => x.OwnerUserId);

            entityTypeBuilder
                .HasIndex(x => x.OwnerRoleId);
        }
    }
}
