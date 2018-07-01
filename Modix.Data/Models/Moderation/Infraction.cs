using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models.Admin
{
    /// <summary>
    /// Describes the result of one or more moderation actions performed upon a user, by a staff member.
    /// </summary>
    public class Infraction
    {
        /// <summary>
        /// A unique identifier for this <see cref="Infraction"/>.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        /// <summary>
        /// The type of <see cref="Infraction"/> recorded.
        /// </summary>
        [Required]
        public InfractionTypes Type { get; set; }

        /// <summary>
        /// The <see cref="User.Id"/> value of <see cref="Subject"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Subject))]
        public ulong SubjectId { get; set; }

        /// <summary>
        /// The user upon which the <see cref="Infraction"/> was applied.
        /// </summary>
        [Required]
        public User Subject { get; set; }

        /// <summary>
        /// The <see cref="ModerationAction"/> entities applicable to this <see cref="Infraction"/>.
        /// </summary>
        public ICollection<ModerationAction> ModerationActions { get; set; }

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
