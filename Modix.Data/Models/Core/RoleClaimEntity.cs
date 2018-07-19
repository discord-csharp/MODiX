using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes a permission mapping that assigns a claim to a particular role, for use in application authorization.
    /// </summary>
    public class RoleClaimEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="RoleClaimEntity"/>.
        /// </summary>
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the guild to which this mapping applies.
        /// </summary>
        [Required]
        public long GuildId { get; set; }

        /// <summary>
        /// The Discord snowflake ID of the role to which <see cref="Claim"/> is being assigned.
        /// </summary>
        [Required]
        public long RoleId { get; set; }

        /// <summary>
        /// The claim that is being assigned to <see cref="RoleId"/>.
        /// </summary>
        [Required]
        public AuthorizationClaim Claim { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required, ForeignKey(nameof(CreateAction))]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The configuration action that created this mapping.
        /// </summary>
        [Required, InverseProperty(nameof(ConfigurationActionEntity.CreatedRoleClaim))]
        public virtual ConfigurationActionEntity CreateAction { get; set; }

        /// <summary>
        /// The <see cref="ConfigurationActionEntity.Id"/> value (if any) of <see cref="CreateAction"/>.
        /// </summary>
        [ForeignKey(nameof(RescindAction))]
        public long? RescindActionId { get; set; }

        /// <summary>
        /// The configuration action (if any) that rescinded this mapping.
        /// </summary>
        [InverseProperty(nameof(ConfigurationActionEntity.RescindedRoleClaim))]
        public virtual ConfigurationActionEntity RescindAction { get; set; }
    }
}
