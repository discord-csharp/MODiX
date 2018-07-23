using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;

using Modix.Data.Models.Core;

namespace Modix.Services.Core
{
    /// <summary>
    /// Provides methods for authorizing an action to be performed, within the context of a scoped request.
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// The unique identifier, within the Discord API, of the authenticated user (if any) that generated the current request.
        /// </summary>
        ulong? CurrentUserId { get; }

        /// <summary>
        /// The unique identifier, within the Discord API, of the guild (if any) form which the current request was generated.
        /// </summary>
        ulong? CurrentGuildId { get; }

        /// <summary>
        /// Automatically configures default claim mappings for a guild, if none yet exist.
        /// Default claims include granting all existing claims to any role that has the Discord "Administrate"
        /// permission, and to the bot user itself.
        /// </summary>
        /// <param name="guild">The guild to be configured.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task AutoConfigureGuildAsync(IGuild guild);

        /// <summary>
        /// Removes all authorization configuration for a guild, by rescinding all of its claim mappings.
        /// </summary>
        /// <param name="guild">The guild to be un-configured.</param>
        /// <returns>A <see cref="Task"/> that will complete when the operation has completed.</returns>
        Task UnConfigureGuildAsync(IGuild guild);

        /// <summary>
        /// A list of authorization claims possessed by the source of the current request.
        /// </summary>
        IReadOnlyCollection<AuthorizationClaim> CurrentClaims { get; }

        /// <summary>
        /// Retrieves the list of claims currently active and mapped to particular user, within a particular guild.
        /// </summary>
        /// <param name="guildUser">The user whose claims are to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed,
        /// containing the requested list of claims.
        /// </returns>
        Task<IReadOnlyCollection<AuthorizationClaim>> GetGuildUserClaimsAsync(IGuildUser guildUser);

        /// <summary>
        /// Loads authentication and authorization data into the service, based on the given guild, user, and role ID values
        /// retrieved from a frontend authentication mechanism.
        /// </summary>
        /// <param name="guildId">The Discord snowflake ID of the guild for which the current user was authenticated.</param>
        /// <param name="roleIds">The Discord snowflake ID values of the roles assigned to the current authenticated user.</param>
        /// <param name="userId">The Discord snowflake ID of the user that was authenticated.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation has completed.
        /// </returns>
        Task OnAuthenticatedAsync(ulong guildId, IEnumerable<ulong> roleIds, ulong userId);

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
        /// <param name="claims">A set of claims to be checked against <see cref="CurrentClaims"/>.</param>
        void RequireClaims(params AuthorizationClaim[] claims);
    }
}
