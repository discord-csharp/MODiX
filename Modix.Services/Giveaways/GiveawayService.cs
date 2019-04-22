using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Discord;

using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Services.Giveaways
{
    /// <summary>
    /// Describes a service that provides operations for hosting giveaways.
    /// </summary>
    public interface IGiveawayService
    {
        /// <summary>
        /// Determines the winners for the supplied giveaway message.
        /// </summary>
        /// <param name="message">The giveaway message that users reacted to.</param>
        /// <param name="count">The number of winners to return.</param>
        /// <returns>
        /// A <see cref="ValueTask"/> that will complete when the operation is complete,
        /// containing a result type that indicates whether the operation succeeded,
        /// including the results if so or an error message if not.
        /// </returns>
        ValueTask<WinnersResult> GetWinnersAsync(IUserMessage message, int count);
    }

    /// <inheritdoc />
    internal class GiveawayService : IGiveawayService
    {
        public GiveawayService(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        /// <inheritdoc />
        public async ValueTask<WinnersResult> GetWinnersAsync(IUserMessage message, int count)
        {
            _authorizationService.RequireClaims(AuthorizationClaim.HostGiveaway);

            if (count <= 0)
            {
                return new WinnersResult("You need to request at least one winner.");
            }

            if (count > MaximumWinners)
            {
                return new WinnersResult($"You can only have a maximum of {MaximumWinners} winners per giveaway.");
            }

            var reactors = await GetReactorsAsync(message);

            if (reactors.Length == 0)
            {
                return new WinnersResult("Cannot choose any winners, because nobody entered the giveaway.");
            }
            else if (reactors.Length <= count)
            {
                var winners = reactors.Select(x => x.Id)
                    .ToImmutableArray();

                return new WinnersResult(winners);
            }
            else
            {
                var winners = DetermineWinners(reactors, count);

                return new WinnersResult(winners);
            }
        }

        private async Task<ImmutableArray<IUser>> GetReactorsAsync(IUserMessage message)
        {
            var reactors = await message.GetReactionUsersAsync(_giveawayEmoji, int.MaxValue)
                .FlattenAsync();

            return reactors.Where(x => !x.IsBot)
                .ToImmutableArray();
        }

        private ImmutableArray<ulong> DetermineWinners(ImmutableArray<IUser> reactors, int count)
        {
            Debug.Assert(reactors.Length > count);

            var winners = new HashSet<ulong>();

            while (winners.Count < count)
            {
                var winner = reactors[_random.Next(count)];
                winners.Add(winner.Id);
            }

            return winners.ToImmutableArray();
        }

        private readonly IAuthorizationService _authorizationService;

        private const int MaximumWinners = 10;

        private static readonly Emoji _giveawayEmoji = new Emoji("🎉");
        private readonly Random _random = new Random();
    }
}
