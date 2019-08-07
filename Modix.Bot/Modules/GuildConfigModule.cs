using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Modix.Bot.Extensions;
using Modix.Bot.Preconditions;
using Modix.Data.Repositories;
using Modix.Services.CommandHelp;

namespace Modix.Bot.Modules
{
    [Name("Guild Configuration")]
    [Summary("Configure guild-specific policies and options.")]
    [Group("guild")]
    [Alias("server")]
    [HelpTags("guild", "server", "config", "configuration", "options")]
    public class GuildConfigModule : ModuleBase
    {
        private readonly IGuildConfigurationRepository _guildConfig;

        public GuildConfigModule(IGuildConfigurationRepository guildConfig)
        {
            _guildConfig = guildConfig;
        }

        [Command("PromotionsMinimum")]
        [Summary("Gets the minimum number of days a user must be a member of the guild before they can be promoted.")]
        [Alias("pm")]
        public async Task GetPromotionMinimumAsync()
        {
            var minimum = await _guildConfig.GetMinimumDaysBeforePromotion(Context.Guild.Id);
            var minimumText = minimum == 0 ? "0 (Disabled)" : minimum.ToString();
            await Context.ReplyWithEmbed($"Minimum Days Before Promotion: {minimumText}");
        }

        [Command("PromotionsMinimum")]
        [Summary("Sets the minimum number of days a user must be a member of the guild before they can be promoted.")]
        [Alias("pm")]
        [Priority(-10)]
        public async Task SetPromotionMinimumAsync(
            [Summary("A positive number representing the number of days befor a user can be promoted. Use '0' to disable the check.")]
            [Between(0, 365)]
            int minimum)
        {
            await _guildConfig.SetMinimumDaysBeforePromotion(Context.Guild.Id, minimum);
            await Context.AddConfirmation();
        }
        
    }
}
