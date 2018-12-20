using System;
using System.Threading.Tasks;

using Discord;

using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.Mentions
{
    public interface IMentionService
    {
        /// <summary>
        /// Determines whether the user can mention the supplied role.
        /// </summary>
        /// <param name="role">The role that the user is trying to mention.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="role"/>.</exception>
        void AuthorizeMention(IRole role);

        /// <summary>
        /// Ensures that the role can be mentioned.
        /// </summary>
        /// <param name="role">The role to be mentioned.</param>
        /// <returns>
        /// A <see cref="Task"/> that will complete when the operation completes,
        /// with a delegate that can be invoked to restore the role to its previous configuration.
        /// </returns>
        Task<Func<Task>> EnsureMentionableAsync(IRole role);
    }

    /// <inheritdoc />
    internal class MentionService : IMentionService
    {
        public MentionService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
        }

        /// <inheritdoc />
        public void AuthorizeMention(IRole role)
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.MentionRestrictedRole);

            if (role is null)
                throw new ArgumentNullException(nameof(role));
        }

        public async Task<Func<Task>> EnsureMentionableAsync(IRole role)
        {
            if (!role.IsMentionable)
            {
                await role.ModifyAsync(x => x.Mentionable = true);

                return async () => await role.ModifyAsync(x => x.Mentionable = false);
            }

            return () => Task.CompletedTask;
        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IAuthorizationService"/> to be used to interact with frontend authentication system, and perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }
    }
}
