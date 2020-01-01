using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Utilities;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a mapping that assigns an arbitrary designation to a particular channel within a guild.
    /// </summary>
    public class DesignatedChannelMappingEntity
    {
        /// <summary>
        /// A unique identifier for this mapping.
        /// </summary>
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which this mapping applies.
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
        /// The channel to which this mapping applies.
        /// </summary>
        public virtual GuildChannelEntity Channel { get; set; } = null!;

        /// <summary>
        /// The type of designation being mapped to <see cref="Channel"/>.
        /// </summary>
        [Required]
        public DesignatedChannelType Type { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> that created this <see cref="DesignatedChannelMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity CreateAction { get; set; } = null!;

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="DeleteAction"/>.
        /// </summary>
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity"/> (if any) that deleted this <see cref="DesignatedChannelMappingEntity"/>.
        /// </summary>
        public virtual ConfigurationActionEntity? DeleteAction { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<DesignatedChannelMappingEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<DesignatedChannelMappingEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<DesignatedChannelMappingEntity>()
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            modelBuilder
                .Entity<DesignatedChannelMappingEntity>()
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<DesignatedChannelMappingEntity>(x => x.CreateActionId);

            modelBuilder
                .Entity<DesignatedChannelMappingEntity>()
                .HasOne(x => x.DeleteAction)
                .WithOne()
                .HasForeignKey<DesignatedChannelMappingEntity>(x => x.DeleteActionId);
        }
    }
}
