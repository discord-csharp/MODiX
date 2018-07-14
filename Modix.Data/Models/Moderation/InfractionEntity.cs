using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Modix.Data.Models.Core;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes the result of one or more moderation actions performed upon a user, by a staff member.
    /// </summary>
    public class InfractionEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="InfractionEntity"/>.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The type of <see cref="InfractionEntity"/> recorded.
        /// </summary>
        [Required]
        public InfractionTypes Type { get; set; }

        /// <summary>
        /// The <see cref="DiscordUserEntity.UserId"/> value of <see cref="Subject"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Subject))]
        public long SubjectId { get; set; }

        /// <summary>
        /// The user upon which the <see cref="InfractionEntity"/> was applied.
        /// </summary>
        [Required]
        public DiscordUserEntity Subject { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> entities applicable to this <see cref="InfractionEntity"/>.
        /// </summary>
        public ICollection<ModerationActionEntity> ModerationActions { get; set; }

        /// <summary>
        /// The duration from <see cref="Created"/>, indicating when the infraction should be considered "expired".
        /// A null value indicates that the action does not expire.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// A message from <see cref="Staffer"/> describing the reason why this moderation action was performed.
        /// </summary>
        [Required]
        public string Reason { get; set; }

        /// <summary>
        /// A flag indicating whether this infraction has been rescinded.
        /// </summary>
        [Required]
        public bool IsRescinded { get; set; }
    }
}
