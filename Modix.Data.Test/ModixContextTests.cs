using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using NUnit.Framework;
using Shouldly;

namespace Modix.Data.Test
{
    [TestFixture]
    public class ModixContextTests
    {
        [Test]
        public void ModixContext_Always_ModelMatchesMigrationsSnapshot()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ModixContext>()
                .UseNpgsql("Bogus connection string: we don't actually need to connect to the DB, just build ourselves a model.");

            var context = new ModixContext(optionsBuilder.Options);

            var differences = context.GetService<IMigrationsModelDiffer>().GetDifferences(
                    context.GetService<IMigrationsAssembly>().ModelSnapshot.Model,
                    context.Model);

            differences.ShouldBeEmpty();
        }
    }
}
