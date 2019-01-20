using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Utilities;

namespace Modix.Data.Models.Tags
{
    /// <summary>
    /// Describes a user-configurable, reproduceable message.
    /// </summary>
    [Table("Tags")]
    internal class TagEntity
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
        public virtual TagActionEntity CreateAction { get; set; }

        /// <summary>
        /// The unique identifier of the action that deleted the tag.
        /// </summary>
        [ForeignKey(nameof(DeleteAction))]
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The action that deleted the tag.
        /// </summary>
        public virtual TagActionEntity DeleteAction { get; set; }

        /// <summary>
        /// The unique string that is used to invoke the tag.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The message that will be displayed when the tag is invoked.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// The number of times that the tag has been invoked.
        /// </summary>
        [Required]
        public uint Uses { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<TagEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<TagEntity>()
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<TagEntity>(x => x.CreateActionId);

            modelBuilder
                .Entity<TagEntity>()
                .HasOne(x => x.DeleteAction)
                .WithOne()
                .HasForeignKey<TagEntity>(x => x.DeleteActionId);

            modelBuilder
                .Entity<TagEntity>()
                .HasIndex(x => x.GuildId);

            modelBuilder
                .Entity<TagEntity>()
                .HasIndex(x => x.Name);
        }
    }
}
