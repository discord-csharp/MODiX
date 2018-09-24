using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Modules
{
    [Group("auth")]
    [Summary("Commands for configuring the authorization system.")]
    public class AuthorizationModule : ModuleBase
    {
        public AuthorizationModule(IAuthorizationService authorizationService)
        {
            AuthorizationService = authorizationService;
        }

        [Command("claims")]
        [Summary("Lists the currently assigned claims for the current user, or a given user.")]
        public async Task Claims(IGuildUser guildUser = null)
        {
            var claims = (guildUser == null)
                    ? AuthorizationService.CurrentClaims
                    : await AuthorizationService.GetGuildUserClaimsAsync(guildUser);

            await ReplyWithClaims(claims);
        }

        [Command("claims")]
        [Summary("Lists the currently assigned claims for the given role.")]
        public async Task Claims(IRole guildRole)
        {
            var claims = await AuthorizationService.GetGuildRoleClaimsAsync(guildRole);
            await ReplyWithClaims(claims);
        }

        private async Task ReplyWithClaims(IEnumerable<AuthorizationClaim> claims)
        {
            await ReplyAsync(claims.Any()
                 ? Format.Code(string.Join("\r\n", claims.Select(x => x.ToString())))
                 : "No claims assigned");
        }

        [Command("claims add")]
        [Summary("Adds a claim mapping to a given role")]
        public Task AddClaimMapping(
            [Summary("The claim to be added.")]
                AuthorizationClaim claim,
            [Summary("The type of claim mapping, E.G. granted or denied.")]
                ClaimMappingType type,
            [Summary("The role to which the claim is to be added.")]
                IRole role)
            => AuthorizationService.AddClaimMapping(role, type, claim);

        [Command("claims add")]
        [Summary("Adds a claim mapping to a given user")]
        public Task AddClaimMapping(
            [Summary("The claim to be added.")]
                AuthorizationClaim claim,
            [Summary("The type of claim mapping, E.G. granted or denied.")]
                ClaimMappingType type,
            [Summary("The user to which the claim is to be added.")]
                IGuildUser user)
            => AuthorizationService.AddClaimMapping(user, type, claim);

        [Command("claims remove")]
        [Summary("Removes a claim mapping from a given role")]
        public Task RemoveClaimMapping(
            [Summary("The claim to be removed.")]
                AuthorizationClaim claim,
            [Summary("The type of claim mapping, E.G. granted or denied.")]
                ClaimMappingType type,
            [Summary("The role from which the claim is to be removed.")]
                IRole role)
            => AuthorizationService.RemoveClaimMapping(role, type, claim);

        [Command("claims remove")]
        [Summary("Removes a claim mapping from a given user")]
        public Task RemoveClaimMapping(
            [Summary("The claim to be removed.")]
                AuthorizationClaim claim,
            [Summary("The type of claim mapping, E.G. granted or denied.")]
                ClaimMappingType type,
            [Summary("The user from which the claim is to be removed.")]
                IGuildUser user)
            => AuthorizationService.RemoveClaimMapping(user, type, claim);

        internal protected IAuthorizationService AuthorizationService { get; }
    }
}
