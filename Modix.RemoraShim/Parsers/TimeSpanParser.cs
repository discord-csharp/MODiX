using System;
using System.Threading;
using System.Threading.Tasks;

using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Results;

namespace Modix.RemoraShim.Parsers
{
    internal class TimeSpanParser : AbstractTypeParser<TimeSpan>
    {
        public override ValueTask<Result<TimeSpan>> TryParse(string value, CancellationToken ct)
        {
            return Parse(value);

            ValueTask<Result<TimeSpan>> Parse(ReadOnlySpan<char> span)
            {
                var result = TimeSpan.Zero;

                if (span.Length <= 1)
                    return FromError();

                var start = 0;

                while (start < span.Length)
                {
                    if (char.IsDigit(span[start]))
                    {
                        var i = start + 1;

                        while (i < span.Length - 1 && char.IsDigit(span[i]))
                            i++;

                        if (!double.TryParse(span[start..i], out var timeQuantity))
                            return FromError();

                        switch (span[i])
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
                                return FromError();
                        }

                        start = i + 1;
                    }
                    else
                    {
                        return FromError();
                    }
                }

                return ValueTask.FromResult(Result<TimeSpan>.FromSuccess(result));

                ValueTask<Result<TimeSpan>> FromError()
                    => ValueTask.FromResult(Result<TimeSpan>.FromError(new ParsingError<TimeSpan>(value)));
            }
        }
    }
}
