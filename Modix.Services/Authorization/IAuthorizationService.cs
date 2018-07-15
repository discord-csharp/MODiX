using System.Collections.Generic;

namespace Modix.Services.Authorization
{
    /// <summary>
    /// Provides methods for authorizing an action to be performed, within the context of a scoped request.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// A list of authorization claims possessed by the source of the current request.
        /// </summary>
        IReadOnlyCollection<AuthorizationClaim> Claims { get; }

        /// <summary>
        /// Requires that there be an authenticated guild for the current request.
        /// </summary>
        void RequireAuthenticatedGuild();

        /// <summary>
        /// Requires that there be an authenticated user for the current request.
        /// </summary>
        void RequireAuthenticatedUser();

        /// <summary>
        /// Requires that the given set of claims be present, for the current request.
        /// </summary>
        /// <param name="claims">A set of claims to be checked against <see cref="Claims"/>.</param>
        void RequireClaims(params AuthorizationClaim[] claims);
    }
}
