using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Modix.Data
{
    [ServiceBinding(ServiceLifetime.Transient)]
    public class ModixContextAutoMigrationBehavior
        : ScopedBehaviorBase
    {
        public ModixContextAutoMigrationBehavior(
                ILogger<ModixContextAutoMigrationBehavior> logger,
                IServiceScopeFactory serviceScopeFactory)
            : base(
                logger,
                serviceScopeFactory) { }

        protected override async Task OnStartingAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            ModixContextLogMessages.ContextMigrating(_logger);
            await serviceProvider.GetRequiredService<ModixContext>().Database.MigrateAsync(cancellationToken);
            ModixContextLogMessages.ContextMigrated(_logger);
        }

        public override Task StopAsync(
                CancellationToken cancellationToken)
            => Task.CompletedTask;

        protected override Task OnStoppingAsync(
                IServiceProvider serviceProvider,
                CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
