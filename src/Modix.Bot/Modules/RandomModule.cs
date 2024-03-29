namespace Modix.Modules
{
    using System;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Interactions;
    using Modix.Services.CommandHelp;

    [ModuleHelp("Random", "A bunch of commands related to randomness.")]
    [Group("random", "A bunch of commands related to randomness.")]
    public class RandomModule : InteractionModuleBase
    {
        private static Embed GetEmbed(object description)
        {
            return new EmbedBuilder()
                .WithTitle("🎱 Magic 8 ball says...")
                .WithDescription(description.ToString())
                .Build();
        }

        [SlashCommand("number", "Gets a random number.")]
        public async Task RandomNumberAsync(
            [Summary(description: "The inclusive minimum possible number.")]
                int min = 0,
            [Summary(description: "The exclusive maximum possible number.")]
                int max = 10)
        {
            if (min >= max)
            {
                await Context.Channel.SendMessageAsync("Maximum number must be greater than the minimum number.");
                return;
            }

            var number = Random.Shared.Next(min, max);

            await FollowupAsync(embed: GetEmbed(number));
        }

        [SlashCommand("coin", "Flips a coin.")]
        public async Task FlipCoinAsync()
        {
            var coin = Random.Shared.Next(0, 2);

            if (coin == 0)
            {
                await FollowupAsync(embed: GetEmbed("Heads"));
            }
            else
            {
                await FollowupAsync(embed: GetEmbed("Tails"));
            }
        }
    }
}
