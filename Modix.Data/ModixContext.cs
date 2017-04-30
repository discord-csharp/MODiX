using System;
using Microsoft.EntityFrameworkCore;
using Modix.Data.Models;

namespace Modix.Data
{
    public class ModixContext : DbContext
    {
        public DbSet<Ban> Bans { get; set; }
        public DbSet<DiscordGuild> Guilds { get; set; }
        public DbSet<DiscordMessage> Messages { get; set; }
        public DbSet<DiscordUser> Users { get; set; }
        public DbSet<ChannelLimit> ChannelLimits { get; set; }

        public ModixContext() { }
        public ModixContext(DbContextOptions<ModixContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("MODIX_DB_CONNECTION"));
            }
        }
    }
}
