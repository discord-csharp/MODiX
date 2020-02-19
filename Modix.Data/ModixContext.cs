using Modix.Data.Models.Core;
using Modix.Data.Models.Emoji;

using Microsoft.EntityFrameworkCore;

namespace Modix.Data
{

    public class ModixContext : DbContext
    {
        public ModixContext(
                DbContextOptions<ModixContext> options)
            : base(options) { }

        // For building fakes during testing
        public ModixContext() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModixContext).Assembly);

            modelBuilder.Entity<PerUserMessageCount>().HasNoKey();
            modelBuilder.Entity<GuildUserParticipationStatistics>().HasNoKey();
            modelBuilder.Entity<SingleEmojiStatsDto>().HasNoKey();
            modelBuilder.Entity<EmojiStatsDto>().HasNoKey();
            modelBuilder.Entity<GuildEmojiStats>().HasNoKey();
            modelBuilder.Entity<MessageCountPerChannel>().HasNoKey();
            modelBuilder.Entity<MessageCountByDate>().HasNoKey();
        }
    }
}
