#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Modix.Services.Tags;

namespace Modix.Bot.AutocompleteHandlers
{
    public class TagAutocompleteHandler : AutocompleteHandler
    {
        private ITagCache? _tagCache;

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var argument = autocompleteInteraction.Data.Current;
            if (!argument.Focused || string.IsNullOrWhiteSpace((string)argument.Value))
                return Task.FromResult(AutocompletionResult.FromSuccess());

            _tagCache ??= services.GetRequiredService<ITagCache>();

            var tags = _tagCache.Search(context.Guild.Id, (string)argument.Value, SlashCommandOptionBuilder.MaxChoiceCount)
                .Select(x => new AutocompleteResult(x, x));

            return Task.FromResult(AutocompletionResult.FromSuccess(tags));
        }
    }
}
