using System.Collections.Immutable;

namespace Modix.Services.Giveaways
{
    public class WinnersResult
    {
        public WinnersResult(ImmutableArray<ulong> winners)
        {
            IsError = false;
            Winners = winners;
        }

        public WinnersResult(string error)
        {
            IsError = true;
            Error = error;
        }

        public ImmutableArray<ulong> Winners { get; }

        public string Error { get; }

        public bool IsError { get; }
    }
}
