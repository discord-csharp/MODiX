using System;
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
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationModule(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [Command("claims")]
        [Summary("Lists the currently assigned claims for the calling user, or given user")]
        public async Task ClaimsAsync(
            [Summary("User whom to list the claims for, if any")]
                [Remainder] DiscordUserEntity user = null)
        {
            var guildUser = await Context.Guild.GetUserAsync(user?.Id ?? Context.User.Id);

            if (guildUser is null)
            {
                await ReplyAsync("Unable to find a user with that ID.");
                return;
            }

            var claims = await _authorizationService.GetGuildUserClaimsAsync(guildUser);

            await ReplyWithClaimsAsync(claims);
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
        public Task AddClaimAsync(
            [Summary("Claim to be added")]
                AuthorizationClaim claim,
            [Summary("Access of a claim, whether granted or denied")]
                ClaimMappingType type,
            [Summary("Role which to add the claim to")]
                IRole role)
        {
            return _authorizationService.AddClaimMappingAsync(role, type, claim);
        }

        [Command("claims add")]
        [Summary("Adds a claim to the given user")]
        public Task AddClaimAsync(
            [Summary("Claim to be added")]
                AuthorizationClaim claim,
            [Summary("Access of a claim, whether granted or denied")]
                ClaimMappingType type,
            [Summary("User to add the claim to")]
                IGuildUser user)
        {
            return _authorizationService.AddClaimMappingAsync(user, type, claim);
        }

        [Command("claims remove")]
        [Summary("Removes a claim from the given role")]
        public Task RemoveClaimAsync(
            [Summary("Claim to be removed")]
                AuthorizationClaim claim,
            [Summary("Access of the claim, whether granted or denied")]
                ClaimMappingType type,
            [Summary("Role from which the claim is to be removed")]
                IRole role)
        {
            return _authorizationService.RemoveClaimMappingAsync(role, type, claim);
        }

        [Command("claims remove")]
        [Summary("Removes a claim from the given user")]
        public Task RemoveClaimAsync(
            [Summary("Claim to be added")]
                AuthorizationClaim claim,
            [Summary("Access of a claim, whether granted or denied")]
                ClaimMappingType type,
            [Summary("User to add the claim to")]
                IGuildUser user)
        {
            return _authorizationService.RemoveClaimMappingAsync(user, type, claim);
        }

        private async Task ReplyWithClaimsAsync(IReadOnlyCollection<AuthorizationClaim> claims)
        {
            if (!claims.Any())
            {
                await ReplyAsync("No claims assigned"); // TODO: Move to resources folder perhaps?
                return;
            }

            // Turn the claims into strings, then put them one under another in a code block.
            var stringifiedClaims = claims.Select(x => x.ToString());
            var joinedClaims = string.Join(Environment.NewLine, stringifiedClaims);

            await ReplyAsync(Format.Code(joinedClaims));
        }
    }
}
