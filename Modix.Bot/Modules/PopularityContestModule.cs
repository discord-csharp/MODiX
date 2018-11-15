using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Modix.Services.PopularityContest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Modules
{
    [Name("Popularity Contest")]
    [Group("contest")]
    [Summary("Commands for holding reaction-based popularity contests in a channel")]
    public class PopularityContestModule : ModuleBase
    {
        private readonly IPopularityContestService _contestService;

        public PopularityContestModule(IPopularityContestService contestService)
        {
            _contestService = contestService;
        }

        [Command("count")]
        [Summary("Count the number of the given reaction on messages within the given channel")]
        public async Task Count([Summary("The reaction to be counted")] IEmote emoteToCount,
            [Summary("The channel to count")] ISocketMessageChannel collectionChannel,
            [Summary("A list of roles that a message author must have at least one of, space separated")] params IRole[] roles)
        {
            await _contestService.CollectDataAsync(emoteToCount, collectionChannel, Context.Channel as ISocketMessageChannel, roles);
        }
    }
}
