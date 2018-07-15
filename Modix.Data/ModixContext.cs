using System;
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

        // For building fakes during testing
        public ModixContext()
        {
        }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<ModerationConfigEntity> ModerationConfigs { get; set; }

        public DbSet<ModerationActionEntity> ModerationActions { get; set; }

        public DbSet<InfractionEntity> Infractions { get; set; }

        public DbSet<PromotionCampaignEntity> PromotionCampaigns { get; set; }

        public DbSet<PromotionCommentEntity> PromotionComments { get; set; }
    }
}