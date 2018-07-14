using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Modix.Data
{
    public class ModixContextDesignFactory : IDesignTimeDbContextFactory<ModixContext>
    {
        public ModixContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ModixContext>();
            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=Modix;User Id=postres");
            return new ModixContext(optionsBuilder.Options);
        }
    }
}