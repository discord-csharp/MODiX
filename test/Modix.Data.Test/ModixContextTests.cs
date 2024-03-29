using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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

            var migrationsAssembly = context.GetService<IMigrationsAssembly>();
            var modelRuntimeInitializer = context.GetService<IModelRuntimeInitializer>();
            var modelDiffer = context.GetService<IMigrationsModelDiffer>();
            var designTimeModel = context.GetService<IDesignTimeModel>();

            var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;

            if (snapshotModel is IMutableModel mutableModel)
            {
                snapshotModel = mutableModel.FinalizeModel();
            }

            if (snapshotModel is not null)
            {
                snapshotModel = modelRuntimeInitializer.Initialize(snapshotModel);
            }

            var hasDifferences = modelDiffer.HasDifferences(
                snapshotModel?.GetRelationalModel(),
                designTimeModel.Model.GetRelationalModel());

            hasDifferences.ShouldBeFalse();
        }
    }
}
