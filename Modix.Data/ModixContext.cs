using System.Linq;
using System.Reflection;

using Modix.Data.Models;
using Modix.Data.Models.Core;
using Modix.Data.Models.Emoji;
using Modix.Data.Models.Moderation;
using Modix.Data.Models.Promotions;
using Modix.Data.Models.Tags;
using Modix.Data.Utilities;

using Microsoft.EntityFrameworkCore;

namespace Modix.Data
{

    public class ModixContext : DbContext
    {
        public ModixContext(DbContextOptions<ModixContext> options) : base(options)
        {
        }

        // For building fakes during testing
        public ModixContext()
        {
        }

        public DbSet<ConfigurationActionEntity> ConfigurationActions { get; set; } = null!;

        public DbSet<BehaviourConfiguration> BehaviourConfigurations { get; set; } = null!;

        public DbSet<UserEntity> Users { get; set; } = null!;

        public DbSet<GuildChannelEntity> GuildChannels { get; set; } = null!;

        public DbSet<GuildRoleEntity> GuildRoles { get; set; } = null!;

        public DbSet<GuildUserEntity> GuildUsers { get; set; } = null!;

        public DbSet<ClaimMappingEntity> ClaimMappings { get; set; } = null!;

        public DbSet<DesignatedChannelMappingEntity> DesignatedChannelMappings { get; set; } = null!;

        public DbSet<DesignatedRoleMappingEntity> DesignatedRoleMappings { get; set; } = null!;

        internal DbSet<MessageEntity> Messages { get; set; } = null!;

        public DbSet<ModerationActionEntity> ModerationActions { get; set; } = null!;

        public DbSet<InfractionEntity> Infractions { get; set; } = null!;

        public DbSet<DeletedMessageEntity> DeletedMessages { get; set; } = null!;

        public DbSet<DeletedMessageBatchEntity> DeletedMessageBatches { get; set; } = null!;

        public DbSet<PromotionCampaignEntity> PromotionCampaigns { get; set; } = null!;

        public DbSet<PromotionCommentEntity> PromotionComments { get; set; } = null!;

        public DbSet<PromotionActionEntity> PromotionActions { get; set; } = null!;

        public DbSet<TagEntity> Tags { get; set; } = null!;

        public DbSet<TagActionEntity> TagActions { get; set; } = null!;

        public DbSet<EmojiEntity> Emoji { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var onModelCreatingMethods = Assembly.GetExecutingAssembly()
                .GetTypes()
                .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(x => !(x.GetCustomAttribute<OnModelCreatingAttribute>() is null));

            foreach(var method in onModelCreatingMethods)
                method.Invoke(null, new [] { modelBuilder });

            modelBuilder.Entity<PerUserMessageCount>().HasNoKey();
            modelBuilder.Entity<GuildUserParticipationStatistics>().HasNoKey();
            modelBuilder.Entity<SingleEmojiStatsDto>().HasNoKey();
            modelBuilder.Entity<EmojiStatsDto>().HasNoKey();
            modelBuilder.Entity<GuildEmojiStats>().HasNoKey();
        }
    }
}
