using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Modix
{
    public class EmoteTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Emote.TryParse(input, out var target))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(target));
            }
            else if (Emoji.TryParse(input, out var emoji))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(emoji));
            }

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Could not recognize emoji \"{input}\""));
        }
    }
}
