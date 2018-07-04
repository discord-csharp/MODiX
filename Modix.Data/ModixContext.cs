using Microsoft.EntityFrameworkCore;
using Modix.Data.Models;

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

        public DbSet<DiscordGuild> Guilds { get; set; }
        public DbSet<DiscordMessage> Messages { get; set; }
        public DbSet<DiscordUser> DiscordUsers { get; set; }
        public DbSet<ChannelLimit> ChannelLimits { get; set; }
        public DbSet<PromotionCampaign> PromotionCampaigns { get; set; }
        public DbSet<PromotionComment> PromotionComments { get; set; }
    }
}