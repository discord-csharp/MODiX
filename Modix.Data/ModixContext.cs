﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        private ModixContext()
        {
        }

        public DbSet<ModerationActionEntity> ModerationActions { get; set; }
        public DbSet<InfractionEntity> Infractions { get; set; }
        public DbSet<DiscordUserEntity> DiscordUsers { get; set; }
        public DbSet<DiscordMessageEntity> DiscordMessages { get; set; }
        public DbSet<DiscordGuildEntity> DiscordGuilds { get; set; }
        public DbSet<ChannelLimitEntity> ChannelLimits { get; set; }
        public DbSet<PromotionCampaignEntity> PromotionCampaigns { get; set; }
        public DbSet<PromotionCommentEntity> PromotionComments { get; set; }

        public bool IsAttached<TEntity>(TEntity entity) where TEntity : class
            => Set<TEntity>().Local.Contains(entity);

        public async Task UpdateEntityPropertiesAsync<TEntity, TProperty>(TEntity entity,
            params Expression<Func<TEntity, TProperty>>[] propertyExpressions) where TEntity : class
        {
            var isAttached = IsAttached(entity);
            if (!isAttached)
                Attach(entity);

            var entityEntry = Entry(entity);
            foreach (var propertyExpression in propertyExpressions)
                entityEntry.Property(propertyExpression).IsModified = true;
            await SaveChangesAsync();

            if (!isAttached)
                entityEntry.State = EntityState.Detached;
        }
    }
}