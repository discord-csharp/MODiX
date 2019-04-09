using Microsoft.EntityFrameworkCore;
using Modix.Data.Utilities;

namespace Modix.Data.Models.Moderation
{
    public class InfractionCount
    {
        public InfractionType Type { get; set; }

        public int Count { get; set; }

        [OnModelCreating]
        internal static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Query<InfractionCount>()
                .Property(x => x.Type)
                .HasConversion<string>();
        }
    }
}
