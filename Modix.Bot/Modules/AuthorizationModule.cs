using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Data.Models.Core;
using Modix.Services.CommandHelp;
using Modix.Services.Core;

namespace Modix.Modules
{
    [Name("Authorization")]
    [Group("auth")]
    [Summary("Commands for configuring the authorization system.")]
    [HelpTags("claims")]
    public class AuthorizationModule : ModuleBase
    {
        public AuthorizationModule(IAuthorizationService authorizationService)
        {
            AuthorizationService = authorizationService;
        }

        [Command("claims")]
        [Summary("Lists the currently assigned claims for the current user, or a given user.")]
        public async Task ClaimsAsync(
            [Summary("The user for whom to list claims, if any.")]
                [Remainder] DiscordUserEntity user = null)
        {
            var guildUser = await Context.Guild.GetUserAsync(user?.Id ?? Context.User.Id);
            var claims = await AuthorizationService.GetGuildUserClaimsAsync(guildUser);

            await ReplyWithClaimsAsync(claims);
        }

        [Command("claims")]
        [Summary("Lists the currently assigned claims for the given role.")]
        public async Task ClaimsAsync(
            [Summary("The role for which to list claims.")]
                IRole guildRole)
        {
            var claims = await AuthorizationService.GetGuildRoleClaimsAsync(guildRole);
            await ReplyWithClaimsAsync(claims);
        }

        private async Task ReplyWithClaimsAsync(IEnumerable<AuthorizationClaim> claims)
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
            [Summary("The type of claim mapping, e.g. granted or denied.")]
                ClaimMappingType type,
            [Summary("The role to which the claim is to be added.")]
                IRole role)
            => AuthorizationService.AddClaimMappingAsync(role, type, claim);

        [Command("claims add")]
        [Summary("Adds a claim mapping to a given user")]
        public Task AddClaimMapping(
            [Summary("The claim to be added.")]
                AuthorizationClaim claim,
            [Summary("The type of claim mapping, e.g. granted or denied.")]
                ClaimMappingType type,
            [Summary("The user to which the claim is to be added.")]
                IGuildUser user)
            => AuthorizationService.AddClaimMappingAsync(user, type, claim);

        [Command("claims remove")]
        [Summary("Removes a claim mapping from a given role")]
        public Task RemoveClaimMapping(
            [Summary("The claim to be removed.")]
                AuthorizationClaim claim,
            [Summary("The type of claim mapping, e.g. granted or denied.")]
                ClaimMappingType type,
            [Summary("The role from which the claim is to be removed.")]
                IRole role)
            => AuthorizationService.RemoveClaimMappingAsync(role, type, claim);

        [Command("claims remove")]
        [Summary("Removes a claim mapping from a given user")]
        public Task RemoveClaimMapping(
            [Summary("The claim to be removed.")]
                AuthorizationClaim claim,
            [Summary("The type of claim mapping, e.g. granted or denied.")]
                ClaimMappingType type,
            [Summary("The user from which the claim is to be removed.")]
                IGuildUser user)
            => AuthorizationService.RemoveClaimMappingAsync(user, type, claim);

        internal protected IAuthorizationService AuthorizationService { get; }
    }
}
