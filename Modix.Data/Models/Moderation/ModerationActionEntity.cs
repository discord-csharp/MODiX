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
        /// The <see cref="InfractionEntity.SubjectId"/> value of <see cref="Infraction"/>.
        /// </summary>
        [ForeignKey(nameof(Infraction))]
        public long? InfractionId { get; set; }

        /// <summary>
        /// The <see cref="Infraction"/> to which this <see cref="ModerationActionEntity"/> applies.
        /// Null, if an <see cref="Infraction"/> was involved in this <see cref="ModerationActionEntity"/>.
        /// </summary>
        [Required]
        public InfractionEntity Infraction { get; set; }

        /// <summary>
        /// A timestamp indicating when this <see cref="ModerationActionEntity"/> occurred.
        /// </summary>
        [Required]
        public DateTimeOffset Created { get; set; }

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
        /// A comment about the moderation action that was performed.
        /// </summary>
        [Required]
        public string Comment { get; set; }
    }
}
