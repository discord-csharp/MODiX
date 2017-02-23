using System;
using System.Collections.Generic;
using System.Text;
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("User ID=modix;Password=modix123;Host=localhost;Port=5432;Database=modix;Pooling=true;");
        }
    }
}
