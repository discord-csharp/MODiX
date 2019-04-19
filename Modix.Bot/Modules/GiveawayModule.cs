using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Humanizer;

using Modix.Services.CommandHelp;

namespace Modix.Bot.Modules
{
    [Name("Giveaways")]
    [Summary("Host timed or untimed giveaways.")]
    [Group("giveaway")]
    [Alias("giveaways")]
    [HelpTags("giveaways")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class GiveawayModule : ModuleBase
    {
        [Command("choose")]
        [Alias("pick")]
        [Summary("Randomly chooses a winner for the supplied giveaway.")]
        public async Task ChooseAsync(
            [Summary("The giveaway message from which users will be drawn.")]
                DiscordUserMessage message,
            [Summary("How many winners to choose.")]
                int count = 1)
        {
            if (count <= 0)
            {
                await ReplyAsync("You need to request at least one winner.");
                return;
            }

            var reactors = await GetReactorsAsync(message);
            ImmutableArray<ulong> winners;

            if (reactors.Length == 0)
            {
                await ReplyAsync("Cannot choose any winners, because nobody entered the giveaway.");
                return;
            }
            else if (reactors.Length <= count)
            {
                winners = reactors.Select(x => x.Id).ToImmutableArray();
            }
            else
            {
                winners = DetermineWinners(reactors, count);
            }

            var mentions = winners.Humanize(id => MentionUtils.MentionUser(id));
            var response = $"Congratulations, {mentions}! You've won!";

            await ReplyAsync(response);
        }

        private async Task<ImmutableArray<IUser>> GetReactorsAsync(DiscordUserMessage message)
        {
            var reactors = await message.GetReactionUsersAsync(_giveawayEmoji, int.MaxValue).FlattenAsync();

            return reactors.Where(x => !x.IsBot).ToImmutableArray();
        }

        private ImmutableArray<ulong> DetermineWinners(ImmutableArray<IUser> reactors, int count)
        {
            Debug.Assert(reactors.Length > count);

            var winners = new HashSet<ulong>();

            while (winners.Count < count)
            {
                var winner = reactors[GetRandomValue(count)];
                winners.Add(winner.Id);
            }

            return winners.ToImmutableArray();
        }

        private static int GetRandomValue(int maxValue)
        {
            lock (_lockObject)
            {
                return _random.Next(maxValue);
            }
        }

        private static readonly Emoji _giveawayEmoji = new Emoji("🎉");
        private static readonly Random _random = new Random();
        private static readonly object _lockObject = new object();
    }
}
