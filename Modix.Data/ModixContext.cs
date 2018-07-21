using System;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models;
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
        public DbSet<BehaviourConfiguration> BehaviourConfigurations { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<BehaviourConfiguration>()
                .Property(x => x.Category)
                .HasConversion(category => category.ToString(), x => (BehaviourCategory)Enum.Parse(typeof(BehaviourCategory), x));
        }

        public bool IsAttached<TEntity>(TEntity entity) where TEntity : class
            => Set<TEntity>().Local.Contains(entity);

        public DbSet<RoleClaimEntity> RoleClaims { get; set; }

        public DbSet<ModerationConfigEntity> ModerationConfigs { get; set; }

        public DbSet<ModerationActionEntity> ModerationActions { get; set; }

        public DbSet<InfractionEntity> Infractions { get; set; }

        public DbSet<PromotionCampaignEntity> PromotionCampaigns { get; set; }

        public DbSet<PromotionCommentEntity> PromotionComments { get; set; }
        public DbSet<NoteEntity> Notes { get; set; }
    }
}