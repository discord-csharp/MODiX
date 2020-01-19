using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Modix.Data.Models.Core;

namespace Modix.Data
{
    public class ModixContextDesignFactory : IDesignTimeDbContextFactory<ModixContext>
    {
        public ModixContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets("5B9ECED8-0E0C-443E-98B1-F1E508AB292B")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ModixContext>()
                .UseNpgsql(configuration.GetValue<string>(nameof(ModixConfig.DbConnection)));

            return new ModixContext(optionsBuilder.Options);
        }
    }
}
