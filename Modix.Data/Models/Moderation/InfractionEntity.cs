﻿using System;
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
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The type of <see cref="InfractionEntity"/> recorded.
        /// </summary>
        [Required]
        public InfractionType Type { get; set; }

        /// <summary>
        /// The duration from <see cref="Created"/>, indicating when the infraction should be considered "expired".
        /// A null value indicates that the action does not expire.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// The <see cref="DiscordUserEntity.UserId"/> value of <see cref="Subject"/>.
        /// </summary>
        [Required, ForeignKey(nameof(Subject))]
        public long SubjectId { get; set; }

        /// <summary>
        /// The user upon which the <see cref="InfractionEntity"/> was applied.
        /// </summary>
        [Required]
        public virtual DiscordUserEntity Subject { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required, ForeignKey(nameof(CreateAction))]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> that created this <see cref="InfractionEntity"/>.
        /// </summary>
        [Required]
        [InverseProperty(nameof(ModerationActionEntity.CreatedInfraction))]
        public virtual ModerationActionEntity CreateAction { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> value of <see cref="RescindAction"/>.
        /// </summary>
        [ForeignKey(nameof(RescindAction))]
        public long? RescindActionId { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> (if any) that rescinded this <see cref="InfractionEntity"/>.
        /// </summary>
        [InverseProperty(nameof(ModerationActionEntity.RescindedInfraction))]
        public virtual ModerationActionEntity RescindAction { get; set; }
    }
}