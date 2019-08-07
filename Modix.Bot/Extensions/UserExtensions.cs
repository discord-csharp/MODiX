using System;
using System.Linq;
using Discord;

namespace Modix.Bot.Extensions
{
    public static class UserExtensions
    {
        /// <summary>
        /// Returns the user's username and description formatted as username#discrim
        /// </summary>
        public static string UsernameAndDiscrim(this IUser user)
            => $"{user.Username}#{user.Discriminator}";

        /// <summary>
        /// Retrieves an avatar url for the player.
        /// </summary>
        /// <returns>The player's avatar url, or if null, the default avatar url for the player</returns>
        public static string GetAvatarUrlOrDefault(this IUser user)
            => user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

        /// <summary>
        /// Determine's whether a user is a member of a role.
        /// </summary>
        /// <param name="role">The role to test against</param>
        /// <returns>True if the user has the role; otherwise, false.</returns>
        public static bool HasRole(this IGuildUser user, IRole role)
            => user.RoleIds.Contains(role.Id);
    }
}
