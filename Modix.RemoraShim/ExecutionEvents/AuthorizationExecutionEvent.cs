using System.Threading;
using System.Threading.Tasks;

using Modix.RemoraShim.Errors;
using Modix.RemoraShim.Services;

using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Modix.RemoraShim.ExecutionEvents
{
    /// <summary>
    /// Ensures that the command user is authenticated in the authorization service before a command is invoked.
    /// </summary>
    internal class AuthorizationExecutionEvent : IPreExecutionEvent
    {
        public AuthorizationExecutionEvent(
            IAuthorizationContextService requestAuthorizationService)
        {
            _requestAuthorizationService = requestAuthorizationService;
        }

        public async Task<Result> BeforeExecutionAsync(ICommandContext context, CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested || !context.GuildID.HasValue)
                return Result.FromError(new OperationCanceledError());

            return await _requestAuthorizationService.SetCurrentAuthenticatedUserAsync(context.GuildID.Value, context.User.ID);
        }

        private readonly IAuthorizationContextService _requestAuthorizationService;
    }
}
