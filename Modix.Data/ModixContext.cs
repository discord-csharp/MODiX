using Microsoft.EntityFrameworkCore;

using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Models.Promotion;

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

        public DbSet<ConfigurationActionEntity> ConfigurationActions { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<ClaimMappingEntity> ClaimMappings { get; set; }

        public DbSet<ModerationConfigEntity> ModerationConfigs { get; set; }

        public DbSet<ModerationActionEntity> ModerationActions { get; set; }

        public DbSet<InfractionEntity> Infractions { get; set; }

        public DbSet<PromotionCampaignEntity> PromotionCampaigns { get; set; }

        public DbSet<PromotionCommentEntity> PromotionComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ClaimMappingEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<ClaimMappingEntity>()
                .Property(x => x.Claim)
                .HasConversion<string>();

            modelBuilder
                .Entity<ConfigurationActionEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<InfractionEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<ModerationActionEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();
        }
    }
}