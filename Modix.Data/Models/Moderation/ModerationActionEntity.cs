﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a moderation action performed by an authorized staff member.
    /// </summary>
    public class ModerationActionEntity
    {
        /// <summary>
        /// A unique identifier for this <see cref="ModerationActionEntity"/>.
        /// </summary>
        [Key, Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// The snowflake ID, within the Discord API, of the guild to which this moderation action applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// A timestamp indicating when this entity was created, for auditing purposes.
        /// </summary>
        [Required]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The type of this <see cref="ModerationActionEntity"/>.
        /// </summary>
        [Required]
        public ModerationActionType Type { get; set; }

        /// <summary>
        /// The <see cref="GuildUserEntity.UserId"/> value of <see cref="CreatedBy"/>.
        /// </summary>
        [Required]
        public ulong CreatedById { get; set; }

        /// <summary>
        /// The staff member that applied this moderation action
        /// </summary>
        [Required]
        public virtual GuildUserEntity CreatedBy { get; set; }

        /// <summary>
        /// The <see cref="InfractionEntity.Id"/> value of <see cref="Infraction"/>.
        /// </summary>
        [ForeignKey(nameof(Infraction))]
        public long? InfractionId { get; set; }

        /// <summary>
        /// The <see cref="InfractionEntity"/> to which this <see cref="ModerationActionEntity"/> applies.
        /// Null, if an <see cref="InfractionEntity"/> was involved in this <see cref="ModerationActionEntity"/>.
        /// </summary>
        public virtual InfractionEntity Infraction { get; set; }

        /// <summary>
        /// The <see cref="DeletedMessageEntity.MessageId"/> value of <see cref="DeletedMessage"/>.
        /// </summary>
        [ForeignKey(nameof(DeletedMessage))]
        public ulong? DeletedMessageId { get; set; }

        /// <summary>
        /// The <see cref="DeletedMessageEntity"/> to which this <see cref="ModerationActionEntity"/> applies.
        /// Null, if an <see cref="DeletedMessageEntity"/> was involved in this <see cref="ModerationActionEntity"/>.
        /// </summary>
        public virtual DeletedMessageEntity DeletedMessage { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ModerationActionEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<ModerationActionEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<ModerationActionEntity>()
                .Property(x => x.CreatedById)
                .HasConversion<long>();

            modelBuilder
                .Entity<ModerationActionEntity>()
                .Property(x => x.DeletedMessageId)
                .HasConversion<long>();

            modelBuilder
                .Entity<ModerationActionEntity>()
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.CreatedById });
        }
    }
}
