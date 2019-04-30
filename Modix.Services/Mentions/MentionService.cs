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
        /// Mentions the given role in the given channel
        /// </summary>
        /// <param name="role">The role to mention</param>
        /// <param name="channel">The channel to mention in</param>
        Task MentionRoleAsync(IRole role, IMessageChannel channel);

        /// <summary>
        /// Mentions the given role in the given channel
        /// </summary>
        /// <param name="role">The role to mention</param>
        /// <param name="channel">The channel to mention in</param>
        /// <param name="message">The message to send alongside the mention</param>
        Task MentionRoleAsync(IRole role, IMessageChannel channel, string message);
    }

    /// <inheritdoc />
    internal class MentionService : IMentionService
    {
        public MentionService(
            IDiscordClient discordClient,
            IAuthorizationService authorizationService,
            IDesignatedRoleService designatedRoleService)
        {
            DiscordClient = discordClient;
            AuthorizationService = authorizationService;
            DesignatedRoleService = designatedRoleService;
        }

        /// <inheritdoc />
        public async Task MentionRoleAsync(IRole role, IMessageChannel channel)
            => await MentionRoleAsync(role, channel, string.Empty);

        /// <inheritdoc />
        public async Task MentionRoleAsync(IRole role, IMessageChannel channel, string message)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            if (channel is null)
                throw new ArgumentNullException(nameof(channel));

            if (role.IsMentionable && string.IsNullOrWhiteSpace(message))
            {
                await channel.SendMessageAsync($"You can do that yourself - but fine: {role.Mention}");
                return;
            }

            AuthorizationService.RequireClaims(AuthorizationClaim.MentionRestrictedRole);

            if (await DesignatedRoleService.RoleHasDesignationAsync(role.Guild.Id, role.Id, DesignatedRoleType.RestrictedMentionability) == false)
            {
                await channel.SendMessageAsync($"Sorry, **{role.Name}** hasn't been designated as mentionable.");
                return;
            }

            //Set the role to mentionable, immediately mention it, then set it
            //to unmentionable again

            await role.ModifyAsync(x => x.Mentionable = true);

            //Make sure we set the role to unmentionable again no matter what
            try
            {
                await channel.SendMessageAsync($"{role.Mention} {message}");
            }
            finally
            {
                await role.ModifyAsync(x => x.Mentionable = false);
            }
        }

        /// <summary>
        /// An <see cref="IDiscordClient"/> for interacting with the Discord API.
        /// </summary>
        internal protected IDiscordClient DiscordClient { get; }

        /// <summary>
        /// An <see cref="IAuthorizationService"/> to be used to interact with frontend authentication system, and perform authorization.
        /// </summary>
        internal protected IAuthorizationService AuthorizationService { get; }

        /// <summary>
        /// An <see cref="IDesignatedRoleService"/> to be used to check if a role is mentionable
        /// </summary>
        public IDesignatedRoleService DesignatedRoleService { get; }
    }
}
