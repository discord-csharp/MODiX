using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a permission mapping that assigns a claim to a particular role or user within a guild, for use in application authorization.
    /// </summary>
    public class ClaimMappingEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="ClaimMappingEntity"/>.
        /// </summary>
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The type of this mapping.
        /// </summary>
        [Required]
        public ClaimMappingType Type { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which this mapping applies.
        /// </summary>
        [Required]
        public long GuildId { get; set; }

        /// <summary>
        /// The Discord snowflake ID (if any) of the role to which <see cref="Claim"/> is being assigned.
        /// </summary>
        public long? RoleId { get; set; }

        /// <summary>
        /// The Discord snowflake ID (if any) of the user to which <see cref="Claim"/> is being assigned.
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// The claim that is being mapped.s
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
        public virtual ConfigurationActionEntity CreateAction { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value (if any) of <see cref="CreateAction"/>.
        /// </summary>
        public long? DeleteActionId { get; set; }

        /// <summary>
        /// The configuration action (if any) that deleted this mapping.
        /// </summary>
        public virtual ConfigurationActionEntity DeleteAction { get; set; }
    }
}
