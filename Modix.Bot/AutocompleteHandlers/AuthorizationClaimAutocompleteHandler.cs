#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Modix.Models.Core;

namespace Modix.Bot.AutocompleteHandlers
{
    public class AuthorizationClaimAutocompleteHandler : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var argument = autocompleteInteraction.Data.Current;
            if (!argument.Focused)
                return Task.FromResult(AutocompletionResult.FromSuccess());

            var matchingClaims = ClaimInfoData.GetClaims().Values
                .Where(x => x.Name.Contains((string)argument.Value, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Name)
                .Take(SlashCommandOptionBuilder.MaxChoiceCount)
                .Select(x => new AutocompleteResult(x.Name, x.Name));

            return Task.FromResult(AutocompletionResult.FromSuccess(matchingClaims));
        }
    }
}
