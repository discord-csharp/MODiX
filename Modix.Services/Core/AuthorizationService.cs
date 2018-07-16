using System.Collections.Generic;

namespace Modix.Services.Core
{
    /// <inheritdoc />
    public class AuthorizationService : IAuthorizationService
    {
        /// <inheritdoc />
        // TODO: Implement this
        public IReadOnlyCollection<AuthorizationClaim> Claims { get; }
            = new AuthorizationClaim[] { };

        /// <inheritdoc />
        // TODO: Implement this
        public void RequireAuthenticatedGuild() { }

        /// <inheritdoc />
        // TODO: Implement this
        public void RequireAuthenticatedUser() { }

        /// <inheritdoc />
        // TODO: Implement this
        public void RequireClaims(params AuthorizationClaim[] claims) { }
    }
}
