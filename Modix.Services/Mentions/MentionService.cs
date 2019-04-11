using System;
using System.Linq;
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
        /// <param name="message">The message that triggered the mention-action</param>
        Task MentionRoleAsync(IRole role, IUserMessage message, string content);
    }

    /// <inheritdoc />
    internal class MentionService : IMentionService
    {
        public MentionService(
            IAuthorizationService authorizationService,
            IDesignatedRoleService designatedRoleService)
        {
            AuthorizationService = authorizationService;
            DesignatedRoleService = designatedRoleService;
        }

        /// <inheritdoc />
        public async Task MentionRoleAsync(IRole role, IUserMessage message, string content)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            if(!(message.Channel is ITextChannel channel) || !(message.Author is IGuildUser user))
            {
                return;
            }

            if (await DesignatedRoleService.RoleHasDesignationAsync(role.Guild.Id, role.Id, DesignatedRoleType.RestrictedMentionability))
            {
                AuthorizationService.RequireClaims(AuthorizationClaim.MentionRestrictedRole);

                await channel.SendMessageAsync($"Sorry, **{role.Name}** hasn't been designated as mentionable.");
                return;
            }

            //Set the role to mentionable, immediately mention it, then set it
            //to unmentionable again (if it isn't a pingable role)

            await role.ModifyAsync(x => x.Mentionable = true);

            var pingMessage = $"{role.Mention} listen up, {user.Mention} wants your attention!";

            pingMessage = content is null ? pingMessage : $"{pingMessage} (s)he left this message for you: {content}";

            //Make sure we set the role to unmentionable again no matter what
            try
            {
                await channel.SendMessageAsync(pingMessage);
            }
            finally
            {
                if (!await DesignatedRoleService.RoleHasDesignationAsync(role.Guild.Id, role.Id, DesignatedRoleType.Pingable))
                {
                    await role.ModifyAsync(x => x.Mentionable = false);
                }
            }
        }

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
