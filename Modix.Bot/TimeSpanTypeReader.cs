using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Modix.Bot
{
    public class TimeSpanTypeReader : TypeReader
    {
        /// <inheritdoc />
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
            => TryParseTimeSpan(input.ToLowerInvariant(), out var timeSpan)
                ? Task.FromResult(TypeReaderResult.FromSuccess(timeSpan))
                : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse TimeSpan"));

        internal protected bool TryParseTimeSpan(ReadOnlySpan<char> input, out TimeSpan result)
        {
            result = TimeSpan.Zero;

            if (input.Length <= 1)
                return false;

            var start = 0;

            while (start < input.Length)
            {
                if (char.IsDigit(input[start]))
                {
                    var i = start + 1;

                    while (i < input.Length - 1 && char.IsDigit(input[i]))
                        i++;

                    if (!double.TryParse(input.Slice(start, i - start), out var timeQuantity))
                        return false;

                    switch (input[i])
                    {
                        case 'w':
                            result += TimeSpan.FromDays(timeQuantity * 7);
                            break;
                        case 'd':
                            result += TimeSpan.FromDays(timeQuantity);
                            break;
                        case 'h':
                            result += TimeSpan.FromHours(timeQuantity);
                            break;
                        case 'm':
                            result += TimeSpan.FromMinutes(timeQuantity);
                            break;
                        case 's':
                            result += TimeSpan.FromSeconds(timeQuantity);
                            break;
                        default:
                            return false;
                    }

                    start = i + 1;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
