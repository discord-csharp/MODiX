﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Moderation
{
    /// <summary>
    /// Describes a message that was automatically deleted by the application.
    /// </summary>
    public class DeletedMessageEntity
    {
        /// <summary>
        /// The snowflake ID, within the Discord API, of the message that was deleted.
        /// </summary>
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong MessageId { get; set; }

        /// <summary>
        /// The snowflake ID, within the Discord API, of the guild to which this infraction applies.
        /// </summary>
        [Required]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The <see cref="GuildChannelEntity.ChannelId"/> value of <see cref="Channel"/>.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Channel))]
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The channel from which the message was deleted.
        /// </summary>
        [Required]
        public virtual GuildChannelEntity Channel { get; set; }
        
        /// <summary>
        /// The <see cref="GuildUserEntity.UserId"/> value of <see cref="Author"/>.
        /// </summary>
        [Required]
        public ulong AuthorId { get; set; }

        /// <summary>
        /// The user that authored the deleted message.
        /// </summary>
        [Required]
        public virtual GuildUserEntity Author { get; set; }

        /// <summary>
        /// The content of the deleted message.
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// A description of the reason that the message was deleted.
        /// </summary>
        [Required]
        public string Reason { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity.Id"/> value of <see cref="CreateAction"/>.
        /// </summary>
        [Required]
        public long CreateActionId { get; set; }

        /// <summary>
        /// The <see cref="ModerationActionEntity"/> that created this <see cref="DeletedMessageEntity"/>.
        /// </summary>
        [Required]
        public virtual ModerationActionEntity CreateAction { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<DeletedMessageEntity>()
                .Property(x => x.MessageId)
                .HasConversion<long>();

            modelBuilder
                .Entity<DeletedMessageEntity>()
                .Property(x => x.GuildId)
                .HasConversion<long>();

            modelBuilder
                .Entity<DeletedMessageEntity>()
                .Property(x => x.ChannelId)
                .HasConversion<long>();

            modelBuilder
                .Entity<DeletedMessageEntity>()
                .Property(x => x.AuthorId)
                .HasConversion<long>();

            modelBuilder
                .Entity<DeletedMessageEntity>()
                .HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.AuthorId });

            modelBuilder
                .Entity<DeletedMessageEntity>()
                .HasOne(x => x.CreateAction)
                .WithOne()
                .HasForeignKey<DeletedMessageEntity>(x => x.CreateActionId);
        }
    }
}
