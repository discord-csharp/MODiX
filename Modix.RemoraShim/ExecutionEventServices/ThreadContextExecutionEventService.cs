using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Modix.RemoraShim.Errors;

using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Modix.RemoraShim.ExecutionEventServices
{
    /// <summary>
    /// Ensures that shim commands are never invoked outside of a thread.
    /// </summary>
    [ServiceBinding(ServiceLifetime.Scoped)]
    internal class ThreadContextExecutionEventService : IExecutionEventService
    {
        public ThreadContextExecutionEventService(
            IDiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
        }

        public Task<Result> AfterExecutionAsync(ICommandContext context, IResult executionResult, CancellationToken ct = default)
            => Task.FromResult(Result.FromSuccess());

        public async Task<Result> BeforeExecutionAsync(ICommandContext context, CancellationToken ct = default)
        {
            var channelResult = await _channelApi.GetChannelAsync(context.ChannelID, ct);
            if (!channelResult.IsSuccess)
                return Result.FromError(channelResult);

            if (channelResult.Entity is { ThreadMetadata: { HasValue: false } })
                return Result.FromError(new NonThreadContextError(context.ChannelID));

            return Result.FromSuccess();
        }

        private readonly IDiscordRestChannelAPI _channelApi;
    }
}
