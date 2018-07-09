using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a moderation action performed by an authorized staff member.
    /// </summary>
    public class ModerationActionEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="Infraction"/>.
        /// </summary>
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The type of this <see cref="ModerationActionEntity"/>.
        /// </summary>
        [Required]
        public ModerationActionType Type { get; set; }

        /// <summary>
        /// A timestamp indicating when this <see cref="ModerationActionEntity"/> occurred.
        /// </summary>
        [Required]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// A comment about the moderation action that was performed.
        /// </summary>
        [Required]
        public string Reason { get; set; }

        /// <summary>
        /// The <see cref="DiscordUserEntity.UserId"/> value of <see cref="CreatedBy"/>.
        /// </summary>
        [Required, ForeignKey(nameof(CreatedBy))]
        public long CreatedById { get; set; }

        /// <summary>
        /// The staff member that applied this moderation action
        /// </summary>
        [Required]
        public virtual DiscordUserEntity CreatedBy { get; set; }

        /// <summary>
        /// The <see cref="InfractionEntity.Id"/> value of <see cref="Infraction"/>.
        /// </summary>
        [ForeignKey(nameof(Infraction))]
        public long? InfractionId { get; set; }

        /// <summary>
        /// The <see cref="Infraction"/> to which this <see cref="ModerationActionEntity"/> applies.
        /// Null, if an <see cref="Infraction"/> was involved in this <see cref="ModerationActionEntity"/>.
        /// </summary>
        public virtual InfractionEntity Infraction { get; set; }

        /// <summary>
        /// For <see cref="Type"/> values of <see cref="ModerationActionType.InfractionCreated"/>,
        /// this is the <see cref="InfractionEntity"/> that was created by this <see cref="ModerationActionEntity"/>,
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the Infraction property above for both
        // the "Create" and "Rescind" relationships, and throw an error
        [InverseProperty(nameof(InfractionEntity.CreateAction))]
        public virtual InfractionEntity CreatedInfraction { get; set; }

        /// <summary>
        /// For <see cref="Type"/> values of <see cref="ModerationActionType.InfractionRescinded"/>,
        /// this is the <see cref="InfractionEntity"/> that was rescinded by this <see cref="ModerationActionEntity"/>,
        /// </summary>
        // This is needed because if we don't manually map an inverse property for this relationship,
        // EF will try and do it automatically, and will try to use the Infraction property above for both
        // the "Create" and "Rescind" relationships, and throw an error
        [InverseProperty(nameof(InfractionEntity.RescindAction))]
        public virtual InfractionEntity RescindedInfraction { get; set; }
    }
}
