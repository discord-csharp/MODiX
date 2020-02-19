using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Modix.Data.Models.Moderation;
using Modix.Data.Models.Promotions;

namespace Modix.Data.Models.Core
{
    /// <summary>
    /// Describes information about a user, that is tracked on a per-guild basis within the application.
    /// </summary>
    [Table("GuildUsers")]
    public class GuildUserEntity
    {
        [ForeignKey(nameof(User))]
        public ulong UserId { get; set; }
        public virtual UserEntity User { get; set; } = null!;

        public ulong GuildId { get; set; }

        /// <summary>
        /// The Discord Nickname value of the user, within the guild.
        /// </summary>
        public string? Nickname { get; set; }

        public DateTimeOffset FirstSeen { get; set; }

        public DateTimeOffset LastSeen { get; set; }

        public ICollection<InfractionEntity> Infractions { get; set; } = new HashSet<InfractionEntity>();
        public ICollection<MessageEntity> Messages { get; set; } = new HashSet<MessageEntity>();
        public ICollection<PromotionCampaignEntity> PromotionCampaigns { get; set; } = new HashSet<PromotionCampaignEntity>();
    }

    public class GuildUserEntityConfiguration
        : IEntityTypeConfiguration<GuildUserEntity>
    {
        public void Configure(
            EntityTypeBuilder<GuildUserEntity> entityTypeBuilder)
        {
            entityTypeBuilder
                .HasKey(x => new { x.GuildId, x.UserId });

            entityTypeBuilder
                .Property(x => x.GuildId)
                .HasConversion<long>();

            entityTypeBuilder
                .Property(x => x.UserId)
                .HasConversion<long>();
        }
    }
}
