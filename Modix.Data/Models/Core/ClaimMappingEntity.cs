using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a permission mapping that assigns a claim to a particular role or user within a guild, for use in application authorization.
    /// </summary>
    [Table("ClaimMappings")]
    public class ClaimMappingEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="ClaimMappingEntity"/>.
        /// </summary>
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The type of mapping being made between the user/role and claim.
        /// </summary>
        [Required]
        public ClaimMappingType Type { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which this mapping applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The Discord snowflake ID (if any) of the role to which <see cref="Claim"/> is being assigned.
        /// </summary>
        public ulong? RoleId { get; set; }

        /// <summary>
        /// The Discord snowflake ID (if any) of the user to which <see cref="Claim"/> is being assigned.
        /// </summary>
        public ulong? UserId { get; set; }

        /// <summary>
        /// The claim that is being mapped.
        /// </summary>
        [Required]
        public AuthorizationClaim Claim { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The configuration action that created this mapping.
        /// </summary>
        [Required]
        public virtual ConfigurationActionEntity CreateAction { get; set; } = null!;

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value (if any) of <see cref="CreateAction"/>.
        /// </summary>
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The configuration action (if any) that deleted this mapping.
        /// </summary>
        public virtual ConfigurationActionEntity? DeleteAction { get; set; }
    }

    public class ClaimMappingEntityConfigurator
        : IEntityTypeConfiguration<ClaimMappingEntity>
    {
        public void Configure(
            EntityTypeBuilder<ClaimMappingEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .Property(x => x.Type)
                .HasConversion<string>();

            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.RoleId)
                .HasConversion<long?>();

            entityTypeBuilder
                .Property(x => x.UserId)
                .HasConversion<long?>();

            entityTypeBuilder
                .Property(x => x.Claim)
                .HasConversion<string>();

            entityTypeBuilder
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<ClaimMappingEntity>(x => x.CreateActionId);

            entityTypeBuilder
                .HasOne(x => x.DeleteAction)
                .WithOne()
                .HasForeignKey<ClaimMappingEntity>(x => x.DeleteActionId);
        }
    }
}
