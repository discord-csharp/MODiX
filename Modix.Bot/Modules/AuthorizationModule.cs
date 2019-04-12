using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Modix.Bot.Extensions;
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
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationModule(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [Command("claims")]
        [Summary("Lists the currently assigned claims for the calling user, or given user")]
        public async Task ClaimsAsync(

            [Summary("User whom to list the claims for, if any")]
            IGuildUser user = null)

        {
            // If a user was provided, get their claims, otherwise get the caller's claims.
            var claims = user is null
                ? _authorizationService.CurrentClaims
                : await _authorizationService.GetGuildUserClaimsAsync(user);

            await ReplyWithClaimsAsync(claims.ToList());
        }

        [Command("claims")]
        [Summary("Lists the currently assigned claims for the given role.")]
        public async Task ClaimsAsync(

            [Summary("Role for which to list claims of.")]
            IRole role)

        {
            var claims = await _authorizationService.GetGuildRoleClaimsAsync(role);
            await ReplyWithClaimsAsync(claims);
        }

        [Command("claims add")]
        [Summary("Adds a claim to the given role")]
        public Task AddClaim(

            [Summary("Claim to be added")]
            AuthorizationClaim claim,

            [Summary("Access of a claim, whether granted or denied")]
            ClaimMappingType type,

            [Summary("Role which to add the claim to")]
            IRole role)

        {
            return _authorizationService.AddClaimMappingAsync(role, type, claim);
        }

        [Command("claims remove")]
        [Summary("Removes a claim from the given role")]
        public Task RemoveClaim(

            [Summary("Claim to be removed")]
            AuthorizationClaim claim,

            [Summary("Access of the claim, whether granted or denied")]
            ClaimMappingType type,

            [Summary("Role from which the claim is to be removed")]
            IRole role)

        {
            return _authorizationService.RemoveClaimMappingAsync(role, type, claim);
        }

        private async Task ReplyWithClaimsAsync(IReadOnlyCollection<AuthorizationClaim> claims)
        {
            if (!claims.Any())
            {
                await ReplyAsync("No claims assigned"); // TODO: Move to resources folder perhaps?
                return;
            }

            await ReplyAsync(claims.ToMessageableList());
        }
    }
}
