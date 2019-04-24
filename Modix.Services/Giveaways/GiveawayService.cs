using System;
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
        /// A <see cref="Task"/> that will complete when the operation completes,
        /// containing a result type that indicates whether the operation succeeded,
        /// including the results if so or an error message if not.
        /// </returns>
        Task<GiveawayResult> GetWinnersAsync(IUserMessage message, int count);
    }

    /// <inheritdoc />
    internal class GiveawayService : IGiveawayService
    {
        public GiveawayService(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        /// <inheritdoc />
        public async Task<GiveawayResult> GetWinnersAsync(IUserMessage message, int count)
        {
            _authorizationService.RequireClaims(AuthorizationClaim.ExecuteGiveaway);

            if (count <= 0)
            {
                return GiveawayResult.FromError("You need to request at least one winner.");
            }

            if (count > MaximumWinners)
            {
                return GiveawayResult.FromError($"You can only have a maximum of {MaximumWinners} winners per giveaway.");
            }

            var reactors = await GetReactorsAsync(message);

            if (reactors.Length == 0)
            {
                return GiveawayResult.FromError("Cannot choose any winners, because nobody entered the giveaway.");
            }
            else if (reactors.Length <= count)
            {
                var winners = reactors.Select(x => x.Id)
                    .ToImmutableArray();

                return GiveawayResult.FromSuccess(winners);
            }
            else
            {
                var winners = DetermineWinners(reactors, count);

                return GiveawayResult.FromSuccess(winners);
            }
        }

        private async Task<ImmutableArray<IUser>> GetReactorsAsync(IUserMessage message)
            => (await message.GetReactionUsersAsync(_giveawayEmoji, int.MaxValue)
                .FlattenAsync())
                .Where(y => !y.IsBot)
                .ToImmutableArray();

        private ImmutableArray<ulong> DetermineWinners(ImmutableArray<IUser> reactors, int count)
        {
            Debug.Assert(reactors.Length > count);

            return reactors
                .OrderBy(_ => _random.Next())
                .Take(count)
                .Select(x => x.Id)
                .ToImmutableArray();
        }

        private readonly IAuthorizationService _authorizationService;

        private const int MaximumWinners = 10;

        private static readonly Emoji _giveawayEmoji = new Emoji("🎉");
        private readonly Random _random = new Random();
    }
}
