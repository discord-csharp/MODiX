using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Modix.Data.Models.Moderation;

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
        // EF will try and do it automatically, and will try to use the ClaimMapping property above for both
        // the "Create" and "Delete" relationships, and throw an error.
        [InverseProperty(nameof(ClaimMappingEntity.CreateAction))]
        public virtual ClaimMappingEntity CreatedClaimMapping { get; set; }

        /// <summary>
        /// Alias for <see cref="ClaimMapping"/> for <see cref="Type"/> values of <see cref="ConfigurationActionType.ClaimMappingDeleted"/>.
        /// Otherwise, null.
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the ClaimMapping property above for both
        // the "Create" and "Delete" relationships, and throw an error.
        [InverseProperty(nameof(ClaimMappingEntity.DeleteAction))]
        public virtual ClaimMappingEntity DeletedClaimMapping { get; set; }

        /// <summary>
        /// The <see cref="ModerationMuteRoleMappingEntity.Id"/> value (if any) of <see cref="ModerationMuteRoleMapping"/>.
        /// </summary>
        [ForeignKey(nameof(ModerationMuteRoleMapping))]
        public long? ModerationMuteRoleMappingId { get; set; }

        /// <summary>
        /// The moderation mute role mapping that was affected by this action, if any.
        /// </summary>
        public ModerationMuteRoleMappingEntity ModerationMuteRoleMapping { get; set; }

        /// <summary>
        /// Alias for <see cref="ModerationMuteRoleMapping"/> for <see cref="Type"/> values of <see cref="ConfigurationActionType.ModerationMuteRoleMappingCreated"/>.
        /// Otherwise, null.
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the ModerationMuteRoleMapping property above for both
        // the "Create" and "Delete" relationships, and throw an error.
        [InverseProperty(nameof(ModerationMuteRoleMappingEntity.CreateAction))]
        public virtual ModerationMuteRoleMappingEntity CreatedModerationMuteRoleMapping { get; set; }

        /// <summary>
        /// Alias for <see cref="ModerationMuteRoleMapping"/> for <see cref="Type"/> values of <see cref="ConfigurationActionType.ModerationMuteRoleMappingDeleted"/>.
        /// Otherwise, null.
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the ModerationMuteRoleMapping property above for both
        // the "Create" and "Delete" relationships, and throw an error.
        [InverseProperty(nameof(ModerationMuteRoleMappingEntity.DeleteAction))]
        public virtual ModerationMuteRoleMappingEntity DeletedModerationMuteRoleMapping { get; set; }

        /// <summary>
        /// The <see cref="ModerationLogChannelMappingEntity.Id"/> value (if any) of <see cref="ModerationLogChannelMapping"/>.
        /// </summary>
        [ForeignKey(nameof(ModerationLogChannelMapping))]
        public long? ModerationLogChannelMappingId { get; set; }

        /// <summary>
        /// The moderation log channel mapping that was affected by this action, if any.
        /// </summary>
        public ModerationLogChannelMappingEntity ModerationLogChannelMapping { get; set; }

        /// <summary>
        /// Alias for <see cref="ModerationLogChannelMapping"/> for <see cref="Type"/> values of <see cref="ConfigurationActionType.ModerationLogChannelMappingCreated"/>.
        /// Otherwise, null.
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the ModerationLogChannelMapping property above for both
        // the "Create" and "Delete" relationships, and throw an error.
        [InverseProperty(nameof(ModerationLogChannelMappingEntity.CreateAction))]
        public virtual ModerationLogChannelMappingEntity CreatedModerationLogChannelMapping { get; set; }

        /// <summary>
        /// Alias for <see cref="ModerationLogChannelMapping"/> for <see cref="Type"/> values of <see cref="ConfigurationActionType.ModerationLogChannelMappingDeleted"/>.
        /// Otherwise, null.
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the ModerationLogChannelMapping property above for both
        // the "Create" and "Delete" relationships, and throw an error.
        [InverseProperty(nameof(ModerationLogChannelMappingEntity.DeleteAction))]
        public virtual ModerationLogChannelMappingEntity DeletedModerationLogChannelMapping { get; set; }
    }
}
