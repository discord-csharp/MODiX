using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Modix.Bot
{
    public class UriTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
            => Uri.TryCreate(input, UriKind.Absolute, out var uri)
                ? Task.FromResult(TypeReaderResult.FromSuccess(uri))
                : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse URI."));
    }
}
