﻿using Microsoft.EntityFrameworkCore;
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

        public DbSet<GuildUserEntity> GuildUsers { get; set; }

        public DbSet<ClaimMappingEntity> ClaimMappings { get; set; }

        public DbSet<ModerationMuteRoleMappingEntity> ModerationMuteRoleMappings { get; set; }

        public DbSet<ModerationLogChannelMappingEntity> ModerationLogChannelMappings { get; set; }

        public DbSet<ModerationActionEntity> ModerationActions { get; set; }

        public DbSet<InfractionEntity> Infractions { get; set; }

        public DbSet<PromotionCampaignEntity> PromotionCampaigns { get; set; }

        public DbSet<PromotionCommentEntity> PromotionComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<BehaviourConfiguration>()
                .Property(x => x.Category)
                .HasConversion<string>();

            modelBuilder
                .Entity<GuildUserEntity>()
                .HasKey(x => new { x.GuildId, x.UserId });

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
                .Entity<ConfigurationActionEntity>()
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.CreatedById });

            modelBuilder
                .Entity<InfractionEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<InfractionEntity>()
                .HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.SubjectId });

            modelBuilder
                .Entity<ModerationActionEntity>()
                .Property(x => x.Type)
                .HasConversion<string>();

            modelBuilder
                .Entity<ModerationActionEntity>()
                .HasOne(X => X.CreatedBy)
                .WithMany()
                .HasForeignKey(x => new { x.GuildId, x.CreatedById });
        }
    }
}