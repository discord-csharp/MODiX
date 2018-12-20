namespace Modix.Modules
{
    using System;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;

    [Group("random"), Name("Random"), Summary("A bunch of random commands")]
    public class RandomModule : ModuleBase
    {
        private static readonly Random _random = new Random();

        private Embed GetEmbed(object description)
        {
            return new EmbedBuilder()
                .WithTitle("🎱 Magic 8 ball says...")
                .WithDescription(description.ToString())
                .Build();
        }

        [Command("number"), Summary("Gets a random number")]
        public async Task RandomNumber(int min = 0, int max = 10)
        {
            if (min >= max)
            {
                await Context.Channel.SendMessageAsync("Maximum number must be greater than the minimum number");
                return;
            }

            var number = _random.Next(min, max);

            await Context.Channel.SendMessageAsync("", embed: GetEmbed(number));
        }

        [Command("coin"), Summary("Flips a coin")]
        public async Task FlipCoin()
        {
            var coin = _random.Next(0, 2);

            if (coin == 0)
            {
                await Context.Channel.SendMessageAsync("", embed: GetEmbed("Heads"));
            }
            else
            {
                await Context.Channel.SendMessageAsync("", embed: GetEmbed("Tails"));
            }
        }

        [Command("pick"), Summary("Picks from a defined list of inputs")]
        public async Task Pick(params string[] inputs)
        {
            if (inputs.Length <= 1)
            {
                await ReplyAsync("There isn't enough for me to choose from!");
                return;
            }

            var random = _random.Next(0, inputs.Length);
            var choice = inputs[random];

            await ReplyAsync("", embed: GetEmbed(choice));
        }
    }
}
