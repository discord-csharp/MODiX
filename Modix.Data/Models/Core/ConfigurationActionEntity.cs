using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes an action that was performed, that somehow changed the application's configuration.
    /// </summary>
    public class ConfigurationActionEntity
    {
        /// <summary>
        /// A unique identifier for this configuration action.
        /// </summary>
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The type of configuration action that was performed.
        /// </summary>
        [Required]
        public ConfigurationActionType Type { get; set; }

        /// <summary>
        /// A timestamp indicating when this configuration action was performed.
        /// </summary>
        [Required]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The <see cref="UserEntity.Id"/> value of <see cref="CreatedBy"/>.
        /// </summary>
        [Required, ForeignKey(nameof(CreatedBy))]
        public long CreatedById { get; set; }

        /// <summary>
        /// The Discord user that performed this action.
        /// </summary>
        [Required]
        public virtual UserEntity CreatedBy { get; set; }

        /// <summary>
        /// The <see cref="ClaimMappingEntity.Id"/> value (if any) of <see cref="ClaimMapping"/>.
        /// </summary>
        [ForeignKey(nameof(ClaimMapping))]
        public long? ClaimMappingId { get; set; }

        /// <summary>
        /// The claim mapping that was affected by this action, if any.
        /// </summary>
        public ClaimMappingEntity ClaimMapping { get; set; }

        /// <summary>
        /// Alias for <see cref="ClaimMapping"/> for <see cref="Type"/> values of <see cref="ConfigurationActionType.ClaimMappingCreated"/>.
        /// Otherwise, null.
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the RoleClaim property above for both
        // the "Create" and "Rescind" relationships, and throw an error.
        [InverseProperty(nameof(ClaimMappingEntity.CreateAction))]
        public virtual ClaimMappingEntity CreatedClaimMapping { get; set; }

        /// <summary>
        /// Alias for <see cref="ClaimMapping"/> for <see cref="Type"/> values of <see cref="ConfigurationActionType.ClaimMappingRescinded"/>.
        /// Otherwise, null.
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the RoleClaim property above for both
        // the "Create" and "Rescind" relationships, and throw an error.
        [InverseProperty(nameof(ClaimMappingEntity.RescindAction))]
        public virtual ClaimMappingEntity RescindedClaimMapping { get; set; }
    }
}
