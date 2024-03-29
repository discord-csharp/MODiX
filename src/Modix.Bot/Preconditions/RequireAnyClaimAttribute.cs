using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Microsoft.Extensions.DependencyInjection;

using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Bot.Preconditions
{
    public class RequireAnyClaimAttribute : PreconditionAttribute
    {
        private readonly string _errorMessage;

        public RequireAnyClaimAttribute(params AuthorizationClaim[] claims)
        {
            Claims = claims;
            _errorMessage = $"Must have at least one of the following claims: {string.Join(", ", Claims)}";
        }

        public AuthorizationClaim[] Claims { get; }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var authorizationService = services.GetRequiredService<IAuthorizationService>();

            var hasAnyClaim = Claims.Any(x => authorizationService.HasClaim(x));

            return hasAnyClaim
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(PreconditionResult.FromError(_errorMessage));
        }
    }
}
