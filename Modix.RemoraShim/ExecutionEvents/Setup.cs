using Microsoft.Extensions.DependencyInjection;

using Remora.Discord.Commands.Extensions;

namespace Modix.RemoraShim.ExecutionEvents
{
    public static class Setup
    {
        public static IServiceCollection AddExecutionEvents(this IServiceCollection services)
            => services
                .AddPreExecutionEvent<AuthorizationExecutionEvent>()
                .AddPreExecutionEvent<ThreadContextExecutionEvent>();
    }
}
