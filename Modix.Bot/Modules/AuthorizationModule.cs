using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Modix.Bot.AutocompleteHandlers;
using Modix.Bot.Extensions;
using Modix.Bot.Preconditions;
using Modix.Data.Models.Core;
using Modix.Services.CommandHelp;
using Modix.Services.Core;

namespace Modix.Modules
{
    [ModuleHelp("Authorization Claims", "Commands for configuring the authorization system.")]
    [Group("claims", "Commands for configuring the authorization system.")]
    [HelpTags("claims")]
    public class AuthorizationModule : InteractionModuleBase
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationModule(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [SlashCommand("list", "Lists the currently assigned claims for a user or role.")]
        public async Task ClaimsAsync(
            [Summary(description: "User or role for whom to list the claims, if any. Defaults to the current user.")]
                IMentionable target = null)
        {
            target ??= Context.User;

            if (target is IGuildUser user)
            {
                var claims = await _authorizationService.GetGuildUserClaimsAsync(user);
                await ReplyWithClaimsAsync(claims);
            }
            else if (target is IRole role)
            {
                var claims = await _authorizationService.GetGuildRoleClaimsAsync(role);
                await ReplyWithClaimsAsync(claims);
            }
            else
            {
                await FollowupAsync($"Unable to identify {target.Mention} as a user or role.", allowedMentions: AllowedMentions.None);
            }
        }

        [SlashCommand("add", "Adds a claim to the given user or role.")]
        [RequireClaims(AuthorizationClaim.AuthorizationConfigure)]
        public async Task AddClaimAsync(
            [Summary(description: "Claim to be added.")]
            [Autocomplete(typeof(AuthorizationClaimAutocompleteHandler))]
                AuthorizationClaim claim,
            [Summary(description: "Whether to grant or deny the claim.")]
                ClaimMappingType type,
            [Summary(description: "User or role that is to be granted or denied the claim.")]
                IMentionable target)
        {
            if (target is IGuildUser user)
            {
                await _authorizationService.AddClaimMappingAsync(user, type, claim);
                await Context.AddConfirmationAsync();
            }
            else if (target is IRole role)
            {
                await _authorizationService.AddClaimMappingAsync(role, type, claim);
                await Context.AddConfirmationAsync();
            }
            else
            {
                await FollowupAsync($"Unable to identify {target.Mention} as a user or role.", allowedMentions: AllowedMentions.None);
            }
        }

        [SlashCommand("remove", "Removes a claim from the given user or role.")]
        [RequireClaims(AuthorizationClaim.AuthorizationConfigure)]
        public async Task RemoveClaimAsync(
            [Summary(description: "Claim to be removed.")]
            [Autocomplete(typeof(AuthorizationClaimAutocompleteHandler))]
                AuthorizationClaim claim,
            [Summary(description: "Whether the claim is currently granted or denied.")]
                ClaimMappingType type,
            [Summary(description: "User or role from which the claim is to be removed.")]
                IMentionable target)
        {
            if (target is IGuildUser user)
            {
                await _authorizationService.RemoveClaimMappingAsync(user, type, claim);
                await Context.AddConfirmationAsync();
            }
            else if (target is IRole role)
            {
                await _authorizationService.RemoveClaimMappingAsync(role, type, claim);
                await Context.AddConfirmationAsync();
            }
            else
            {
                await FollowupAsync($"Unable to identify {target.Mention} as a user or role.", allowedMentions: AllowedMentions.None);
            }
        }

        private async Task ReplyWithClaimsAsync(IReadOnlyCollection<AuthorizationClaim> claims)
        {
            if (!claims.Any())
            {
                await FollowupAsync("No claims assigned.");
                return;
            }

            // Turn the claims into strings, then put them one under another in a code block.
            var stringifiedClaims = claims.Select(x => x.ToString()).OrderBy(x => x);
            var joinedClaims = string.Join(Environment.NewLine, stringifiedClaims);

            await FollowupAsync(Format.Code(joinedClaims), allowedMentions: AllowedMentions.None);
        }
    }
}
