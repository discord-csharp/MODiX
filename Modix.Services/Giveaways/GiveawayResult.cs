using System.Collections.Immutable;

namespace Modix.Services.Giveaways
{
    public class GiveawayResult
    {
        public static GiveawayResult FromSuccess(ImmutableArray<ulong> winnerIds)
            => new GiveawayResult() { WinnerIds = winnerIds };

        public static GiveawayResult FromError(string error)
            => new GiveawayResult() { Error = error };

        public ImmutableArray<ulong> WinnerIds { get; set; }

        public string Error { get; set; }

        public bool IsError => !(Error is null);
    }
}
