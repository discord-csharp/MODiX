using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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

        public DesignatedChannelModule(IAuthorizationService authorizationService, IDesignatedChannelService designatedChannelService)
        {
            AuthorizationService = authorizationService;
            DesignatedChannelService = designatedChannelService;
        }

        [Command]
        [Summary("Lists all of the channels designated for use by the bot")]
        public async Task List()
        {
            var channels = await DesignatedChannelService.GetDesignatedChannelsAsync(Context.Guild.Id);

            var builder = new EmbedBuilder()
            {
                Title = "Assigned Channel Designations",
                Url = "https://mod.gg/config/channels",
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
                    Name = Format.Bold(_designatedChannelTypeRenderings[type]),
                    Value = (designatedChannels.Length == 0)
                        ? Format.Italics("No channels assigned")
                        : designatedChannels
                            .Select(x => MentionUtils.MentionChannel(x.Channel.Id))
                            .Aggregate(string.Empty, (x, y) => $"{x}\n{y}"),
                    IsInline = false
                });
            }

            await ReplyAsync(string.Empty, false, builder.Build());
        }

        [Command("add")]
        [Summary("Assigns a designation to the given channel")]
        public Task Add(
            [Summary("The channel to be assigned a designation")]
                IMessageChannel channel,
            [Summary("The designation to assign")]
                DesignatedChannelType designation)
            => DesignatedChannelService.AddDesignatedChannelAsync(Context.Guild, channel, designation);

        [Command("add")]
        [Summary("Assigns a designation to the current channel")]
        public Task Add(
            [Summary("The designation to assign")]
                DesignatedChannelType designation)
            => DesignatedChannelService.AddDesignatedChannelAsync(Context.Guild, Context.Channel, designation);

        [Command("remove")]
        [Summary("Removes a designation from the given channel")]
        public Task Remove(
            [Summary("The channel whose designation is to be unassigned")]
                IMessageChannel channel,
            [Summary("The designation to be unassigned")]
                DesignatedChannelType designation)
            => DesignatedChannelService.RemoveDesignatedChannelAsync(Context.Guild, channel, designation);

        [Command("remove")]
        [Summary("Removes a designation from the current channel")]
        public Task Remove(
            [Summary("The designation to be unassigned")]
                DesignatedChannelType designation)
            => DesignatedChannelService.RemoveDesignatedChannelAsync(Context.Guild, Context.Channel, designation);

        private static readonly Dictionary<DesignatedChannelType, string> _designatedChannelTypeRenderings
            = new Dictionary<DesignatedChannelType, string>()
            {
                { DesignatedChannelType.MessageLog,             "Message Log" },
                { DesignatedChannelType.ModerationLog,          "Moderation Log" },
                { DesignatedChannelType.PromotionLog,           "Promotion Log" },
                { DesignatedChannelType.PromotionNotifications, "Promotion Notifications" },
                { DesignatedChannelType.Unmoderated,            "Unmoderated" },
                { DesignatedChannelType.Starboard,              "Starboard" }
            };
    }
}
