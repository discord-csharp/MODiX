using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Microsoft.Extensions.Options;
using Modix.Bot.Extensions;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Modules
{
    [Name("Channel Designations")]
    [Summary("Configures channel designation for various bot services")]
    [Group("channel designations")]
    public class DesignatedChannelModule : ModuleBase
    {
        public IAuthorizationService AuthorizationService { get; }
        public IDesignatedChannelService DesignatedChannelService { get; }

        public ModixConfig Config { get; }

        public DesignatedChannelModule(IAuthorizationService authorizationService, IDesignatedChannelService designatedChannelService, IOptions<ModixConfig> config)
        {
            AuthorizationService = authorizationService;
            DesignatedChannelService = designatedChannelService;
            Config = config.Value;
        }

        [Command]
        [Summary("Lists all of the channels designated for use by the bot")]
        public async Task ListAsync()
        {
            var channels = await DesignatedChannelService.GetDesignatedChannelsAsync(Context.Guild.Id);

            // https://mod.gg/config/channels
            var url = new UriBuilder(Config.WebsiteBaseUrl)
            {
                Path = "/config/channels"
            }.RemoveDefaultPort().ToString();

            var builder = new EmbedBuilder()
            {
                Title = "Assigned Channel Designations",
                Url = url,
                Color = Color.Gold,
                Timestamp = DateTimeOffset.UtcNow
            };

            foreach (var type in Enum.GetValues(typeof(DesignatedChannelType)).Cast<DesignatedChannelType>())
            {
                var designatedChannels = channels
                    .Where(x => x.Type == type)
                    .ToArray();

                builder.AddField(new EmbedFieldBuilder()
                {
                    Name = type.Humanize(),
                    Value = (designatedChannels.Length == 0)
                        ? Format.Italics("No channels assigned")
                        : string.Join(Environment.NewLine, designatedChannels.Select(x => MentionUtils.MentionChannel(x.Channel.Id))),
                    IsInline = false
                });
            }

            await ReplyAsync(embed: builder.Build());
        }

        [Command("add")]
        [Summary("Assigns a designation to the given channel")]
        public async Task AddAsync(
            [Summary("The channel to be assigned a designation")]
                IMessageChannel channel,
            [Summary("The designation to assign")]
                DesignatedChannelType designation)
        {
            await DesignatedChannelService.AddDesignatedChannelAsync(Context.Guild, channel, designation);
            await Context.AddConfirmation();
        }

        [Command("add")]
        [Summary("Assigns a designation to the current channel")]
        public async Task AddAsync(
            [Summary("The designation to assign")]
                DesignatedChannelType designation)
        {
            await DesignatedChannelService.AddDesignatedChannelAsync(Context.Guild, Context.Channel, designation);
            await Context.AddConfirmation();
        }

        [Command("remove")]
        [Summary("Removes a designation from the given channel")]
        public async Task RemoveAsync(
            [Summary("The channel whose designation is to be unassigned")]
                IMessageChannel channel,
            [Summary("The designation to be unassigned")]
                DesignatedChannelType designation)
        {
            await DesignatedChannelService.RemoveDesignatedChannelAsync(Context.Guild, channel, designation);
            await Context.AddConfirmation();
        }

        [Command("remove")]
        [Summary("Removes a designation from the current channel")]
        public async Task RemoveAsync(
            [Summary("The designation to be unassigned")]
                DesignatedChannelType designation)
        {
            await DesignatedChannelService.RemoveDesignatedChannelAsync(Context.Guild, Context.Channel, designation);
            await Context.AddConfirmation();
        }
    }
}
