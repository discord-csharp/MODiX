#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Modix.Modules;

namespace Modix.Bot.AutocompleteHandlers
{
    public class EmojiTypeAutocompleteHandler : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var argument = autocompleteInteraction.Data.Current;
            if (!argument.Focused)
                return Task.FromResult(AutocompletionResult.FromSuccess());

            var matchingResults = Enum.GetNames<EmojiTypeFilter>()
                .Where(x => x.Contains(argument.Value.ToString()!, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x)
                .Take(SlashCommandOptionBuilder.MaxChoiceCount)
                .Select(x => new AutocompleteResult(x, x));

            return Task.FromResult(AutocompletionResult.FromSuccess(matchingResults));
        }
    }
}
