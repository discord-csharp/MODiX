using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
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
            var dependencies = context.GetService<ProviderConventionSetBuilderDependencies>();
            var relationalDependencies = context.GetService<RelationalConventionSetBuilderDependencies>();
            var modelDiffer = context.GetService<IMigrationsModelDiffer>();

            var hasDifferences = false;

            if (migrationsAssembly.ModelSnapshot != null)
            {
                var typeMappingConvention = new TypeMappingConvention(dependencies);
                typeMappingConvention.ProcessModelFinalizing(((IConventionModel)migrationsAssembly.ModelSnapshot.Model).Builder, null!);

                var relationalModelConvention = new RelationalModelConvention(dependencies, relationalDependencies);
                var sourceModel = relationalModelConvention.ProcessModelFinalized(migrationsAssembly.ModelSnapshot.Model);

                hasDifferences = modelDiffer.HasDifferences(
                    ((IMutableModel)sourceModel).FinalizeModel().GetRelationalModel(),
                    context.Model.GetRelationalModel());
            }

            hasDifferences.ShouldBeFalse();
        }
    }
}
