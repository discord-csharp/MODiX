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
    public class RequireClaimsAttribute : PreconditionAttribute
    {
        public RequireClaimsAttribute(params AuthorizationClaim[] claims)
        {
            Claims = claims;
        }

        public AuthorizationClaim[] Claims { get; }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var authorizationService = services.GetRequiredService<IAuthorizationService>();

            var missingClaims = Claims.Where(x => !authorizationService.HasClaim(x)).ToArray();

            return missingClaims.Any()
                ? Task.FromResult(PreconditionResult.FromError($"Missing the following claims: {string.Join(", ", missingClaims)}"))
                : Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
