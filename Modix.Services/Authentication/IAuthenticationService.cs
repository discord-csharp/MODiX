namespace Modix.Services.Authentication
{
    /// <summary>
    /// Provides methods for interacting with an authenticated user, within the context of a scoped request.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// The unique identifier, within the Discord API, of the authenticated user (if any) that generated the current request.
        /// </summary>
        ulong? CurrentUserId { get; }

        /// <summary>
        /// The unique identifier, within the Discord API, of the guild (if any) form which the current request was generated.
        /// </summary>
        ulong? CurrentGuildId { get; }
    }
}
