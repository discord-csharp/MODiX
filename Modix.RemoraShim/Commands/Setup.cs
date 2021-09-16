using Microsoft.Extensions.DependencyInjection;

using Modix.RemoraShim.ExecutionEvents;
using Modix.RemoraShim.Parsers;

using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;

namespace Modix.RemoraShim.Commands
{
    public static class Setup
    {
        public static IServiceCollection AddCommands(this IServiceCollection services)
            => services
                .AddDiscordCommands()
                .AddParsers()
                .AddExecutionEvents()
                .AddCommandGroup<ModerationCommands>()
                .AddCommandGroup<MessageCheckCommands>();
    }
}
