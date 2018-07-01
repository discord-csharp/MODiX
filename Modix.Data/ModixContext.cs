using Microsoft.EntityFrameworkCore;

using Modix.Data.Models;
using Modix.Data.Models.Admin;

namespace Modix.Data
{
    public class ModixContext : DbContext
    {
        public ModixContext(DbContextOptions<ModixContext> options): base(options) { }

        // TODO: Deprecate and remove all these?
        public DbSet<Ban> Bans { get; set; }
        public DbSet<DiscordGuild> Guilds { get; set; }
        public DbSet<DiscordMessage> Messages { get; set; }
        public DbSet<DiscordUser> DiscordUsers { get; set; }
        public DbSet<ChannelLimit> ChannelLimits { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<ModerationAction> ModerationActions { get; set; }

        public DbSet<Infraction> Infractions { get; set; }
    }
}
