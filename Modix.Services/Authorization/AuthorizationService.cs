using System.Collections.Generic;

namespace Modix.Services.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        // TODO: Implement this
        public IReadOnlyCollection<AuthorizationClaim> Claims { get; }
            = new AuthorizationClaim[] { };

        // TODO: Implement this
        public void RequireAuthenticatedGuild() { }

        // TODO: Implement this
        public void RequireAuthenticatedUser() { }

        // TODO: Implement this
        public void RequireClaims(params AuthorizationClaim[] claims) { }
    }
}
