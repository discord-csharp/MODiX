using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Humanizer;
using Microsoft.Extensions.Options;
using Modix.Bot.Extensions;
using Modix.Bot.Preconditions;
using Modix.Common.Extensions;
using Modix.Data.Models.Core;
using Modix.Services;
using Modix.Services.CommandHelp;

namespace Modix.Bot.Modules
{
    [ModuleHelp("Channel Designations", "Configures channel designation for various bot services.")]
    [Group("channel-designations", "Configures channel designation for various bot services.")]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public class DesignatedChannelsModule : InteractionModuleBase
    {
        private readonly DesignatedChannelService _designatedChannelService;
        private readonly ModixConfig _config;

        public DesignatedChannelsModule(DesignatedChannelService designatedChannelService, IOptions<ModixConfig> config)
        {
            _designatedChannelService = designatedChannelService;
            _config = config.Value;
        }

        [SlashCommand("list", "Lists all of the channels designated for use by the bot.")]
        [RequireClaims(AuthorizationClaim.DesignatedChannelMappingRead)]
        public async Task List()
        {
            var channels = await _designatedChannelService.GetDesignatedChannels(Context.Guild.Id);

            var builder = new EmbedBuilder()
            {
                Title = "Assigned Channel Designations",
                Color = Color.Gold,
                Timestamp = DateTimeOffset.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(_config.WebsiteBaseUrl))
            {
                var url = new UriBuilder(_config.WebsiteBaseUrl)
                {
                    Path = "/config/channels"
                }.RemoveDefaultPort().ToString();

                builder.Url = url;
            }

            foreach (var type in Enum.GetValues<DesignatedChannelType>())
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

            await FollowupAsync(embed: builder.Build());
        }

        [SlashCommand("add", "Assigns a designation to the given channel.")]
        [RequireClaims(AuthorizationClaim.DesignatedChannelMappingCreate)]
        public async Task Add(
            [Summary(description: "The channel to be assigned a designation.")]
                IMessageChannel channel,
            [Summary(description: "The designation to assign.")]
                DesignatedChannelType designation)
        {
            await _designatedChannelService.AddDesignatedChannel(Context.Guild, channel, designation);
            await Context.AddConfirmationAsync();
        }

        [SlashCommand("remove", "Removes a designation from the given channel.")]
        [RequireClaims(AuthorizationClaim.DesignatedChannelMappingDelete)]
        public async Task Remove(
            [Summary(description: "The channel whose designation is to be unassigned.")]
                IMessageChannel channel,
            [Summary(description: "The designation to be unassigned.")]
                DesignatedChannelType designation)
        {
            await _designatedChannelService.RemoveDesignatedChannel(Context.Guild, channel, designation);
            await Context.AddConfirmationAsync();
        }
    }
}
