using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Modix.Data
{
    public interface IModixContextAutoMigrationStartupAction
        : IScopedStartupAction { }

    public class ModixContextAutoMigrationStartupAction
        : ScopedStartupActionBase,
            IModixContextAutoMigrationStartupAction
    {
        public ModixContextAutoMigrationStartupAction(
                ILogger<ModixContextAutoMigrationStartupAction> logger,
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
    }
}
