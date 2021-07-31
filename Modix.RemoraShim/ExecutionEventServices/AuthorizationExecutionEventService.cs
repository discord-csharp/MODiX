using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Modix.RemoraShim.Errors;
using Modix.RemoraShim.Services;

using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Modix.RemoraShim.ExecutionEventServices
{
    /// <summary>
    /// Ensures that the command user is authenticated in the authorization service before a command is invoked.
    /// </summary>
    [ServiceBinding(ServiceLifetime.Scoped)]
    internal class AuthorizationExecutionEventService : IExecutionEventService
    {
        public AuthorizationExecutionEventService(
            IAuthorizationContextService requestAuthorizationService)
        {
            _requestAuthorizationService = requestAuthorizationService;
        }

        public Task<Result> AfterExecutionAsync(ICommandContext context, IResult executionResult, CancellationToken ct = default)
            => Task.FromResult(Result.FromSuccess());

        public async Task<Result> BeforeExecutionAsync(ICommandContext context, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested || !context.GuildID.HasValue)
                return Result.FromError(new OperationCanceledError());

            return await _requestAuthorizationService.SetCurrentAuthenticatedUserAsync(context.GuildID.Value, context.User.ID);
        }

        private readonly IAuthorizationContextService _requestAuthorizationService;
    }
}
