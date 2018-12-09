using System.Linq;
using System.Reflection;

using Modix.Data.Models;
using Modix.Data.Models.Core;
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

        public DbSet<ConfigurationActionEntity> ConfigurationActions { get; set; }

        public DbSet<BehaviourConfiguration> BehaviourConfigurations { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<GuildChannelEntity> GuildChannels { get; set; }

        public DbSet<GuildRoleEntity> GuildRoles { get; set; }

        public DbSet<GuildUserEntity> GuildUsers { get; set; }

        public DbSet<ClaimMappingEntity> ClaimMappings { get; set; }

        public DbSet<DesignatedChannelMappingEntity> DesignatedChannelMappings { get; set; }

        public DbSet<DesignatedRoleMappingEntity> DesignatedRoleMappings { get; set; }

        public DbSet<MessageEntity> Messages { get; set; }

        public DbSet<ModerationActionEntity> ModerationActions { get; set; }

        public DbSet<InfractionEntity> Infractions { get; set; }

        public DbSet<DeletedMessageEntity> DeletedMessages { get; set; }

        public DbSet<DeletedMessageBatchEntity> DeletedMessageBatches { get; set; }

        public DbSet<PromotionCampaignEntity> PromotionCampaigns { get; set; }

        public DbSet<PromotionCommentEntity> PromotionComments { get; set; }

        public DbSet<PromotionActionEntity> PromotionActions { get; set; }

        internal DbSet<TagEntity> Tags { get; set; }

        internal DbSet<TagActionEntity> TagActions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var onModelCreatingMethods = Assembly.GetExecutingAssembly()
                .GetTypes()
                .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(x => !(x.GetCustomAttribute<OnModelCreatingAttribute>() is null));

            foreach(var method in onModelCreatingMethods)
                method.Invoke(null, new [] { modelBuilder });
        }
    }
}
