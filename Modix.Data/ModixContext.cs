using Microsoft.EntityFrameworkCore;

using Modix.Data.Models;
using Modix.Data.Models.Admin;

namespace Modix.Data
{
    public class ModixContext : DbContext
    {
        public ModixContext(DbContextOptions<ModixContext> options): base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<ModerationAction> ModerationActions { get; set; }

        public DbSet<Infraction> Infractions { get; set; }
    }
}
