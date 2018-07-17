namespace Modix.Services.Core
{
    /// <inheritdoc />
    public class AuthenticationService : IAuthenticationService
    {
        /// <inheritdoc />
        public ulong? CurrentUserId { get; }

        /// <inheritdoc />
        public ulong? CurrentGuildId { get; }
    }       
}
