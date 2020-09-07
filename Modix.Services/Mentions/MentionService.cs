using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.Mentions
{
    public interface IMentionService
    {
        Task<bool> MentionRole(IRole role, IMessageChannel channel, string message = null);
    }

    [ServiceBinding(ServiceLifetime.Scoped)]
    internal class MentionService : IMentionService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDesignatedRoleService _designatedRoleService;

        public MentionService(IAuthorizationService authorizationService,
            IDesignatedRoleService designatedRoleService)
        {
            _authorizationService = authorizationService;
            _designatedRoleService = designatedRoleService;
        }

        public async Task<bool> MentionRole(IRole role, IMessageChannel channel, string message = null)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            if (channel is null)
                throw new ArgumentNullException(nameof(channel));

            if (message is null)
                message = string.Empty;

            if (role.IsMentionable)
            {
                await channel.SendMessageAsync($"{role.Mention} {message}");
                return true;
            }

            _authorizationService.RequireClaims(AuthorizationClaim.MentionRestrictedRole);

            if (await _designatedRoleService.RoleHasDesignationAsync(role.Guild.Id, role.Id, DesignatedRoleType.RestrictedMentionability, CancellationToken.None) == false)
            {
                await channel.SendMessageAsync($"Sorry, **{role.Name}** hasn't been designated as mentionable.");
                return false;
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

            return true;
        }
    }
}
