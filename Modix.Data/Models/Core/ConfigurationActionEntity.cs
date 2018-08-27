using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Modix.Data.Models.Core;
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
        /// The snowflake ID, within the Discord API, of the guild to which this configuration action applies.
        /// </summary>
        [Required]
        public long GuildId { get; set; }

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
        /// The <see cref="GuildUserEntity.UserId"/> value of <see cref="CreatedBy"/>.
        /// </summary>
        [Required]
        public long CreatedById { get; set; }

        /// <summary>
        /// The Discord user that performed this action.
        /// </summary>
        [Required]
        public virtual GuildUserEntity CreatedBy { get; set; }

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
        /// The <see cref="DesignatedChannelMappingEntity.Id"/> value (if any) of <see cref="DesignatedChannelMappingEntity"/>.
        /// </summary>
        [ForeignKey(nameof(DesignatedChannelMappingEntity))]
        public long? DesignatedChannelMappingId { get; set; }

        /// <summary>
        /// The designated channel mapping that was affected by this action, if any.
        /// </summary>
        public DesignatedChannelMappingEntity DesignatedChannelMapping { get; set; }

        /// <summary>
        /// Alias for <see cref="DesignatedChannelMappingEntity"/> for <see cref="Type"/> values of <see cref="ConfigurationActionType.DesignatedChannelMappingCreated"/>.
        /// Otherwise, null.
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the DesignatedChannelMappingEntity property above for both
        // the "Create" and "Delete" relationships, and throw an error.
        [InverseProperty(nameof(DesignatedChannelMappingEntity.CreateAction))]
        public virtual DesignatedChannelMappingEntity CreatedDesignatedChannelMapping { get; set; }

        /// <summary>
        /// Alias for <see cref="DesignatedChannelMappingEntity"/> for <see cref="Type"/> values of <see cref="ConfigurationActionType.DesignatedChannelMappingDeleted"/>.
        /// Otherwise, null.
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the DesignatedChannelMappingEntity property above for both
        // the "Create" and "Delete" relationships, and throw an error.
        [InverseProperty(nameof(DesignatedChannelMappingEntity.DeleteAction))]
        public virtual DesignatedChannelMappingEntity DeletedDesignatedChannelMapping { get; set; }

        /// <summary>
        /// The <see cref="DesignatedRoleMappingEntity.Id"/> value (if any) of <see cref="DesignatedRoleMappingEntity"/>.
        /// </summary>
        [ForeignKey(nameof(DesignatedRoleMapping))]
        public long? DesignatedRoleMappingId { get; set; }

        /// <summary>
        /// The designated role mapping that was affected by this action, if any.
        /// </summary>
        public DesignatedRoleMappingEntity DesignatedRoleMapping { get; set; }
    }
}
