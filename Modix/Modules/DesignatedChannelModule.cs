using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Modix.Data.Models.Core;
using Modix.Services.Core;

namespace Modix.Modules
{
    [Name("Channel Designation"), Summary("Configures channel designation for various bot services")]
    [Group("designations")]
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
        [Summary("Lists all possible designations, and channels currently assigned to them")]
        public async Task GetLogChannels()
        {
            AuthorizationService.RequireClaims(AuthorizationClaim.ChannelDesignationRead);

            var channelData = await DesignatedChannelService.GetDesignatedChannels(Context.Guild.Id);

            var builder = new EmbedBuilder().WithTitle("Assigned Channel Designations");

            foreach (ChannelDesignation designation in Enum.GetValues(typeof(ChannelDesignation)))
            {
                var channelsWithDesignation = channelData
                    .Where(d => d.ChannelDesignation == designation)
                    .Select(d => MentionUtils.MentionChannel(d.ChannelId))
                    .ToList();

                var newField = new EmbedFieldBuilder().WithName($"**{designation}**");

                if (channelsWithDesignation.Any())
                {
                    newField = newField.WithValue(string.Join(", ", channelsWithDesignation));
                }
                else
                {
                    newField = newField.WithValue("None Assigned");
                }

                builder.AddField(newField);
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("add")]
        [Summary("Assigns a designation to the given channel")]
        public Task AddLogChannel(
            [Summary("The channel to assign")]
                IMessageChannel channel,
            [Summary("The designation to assign")]
                ChannelDesignation designation)
            => DesignatedChannelService.AddDesignatedChannelAsync(Context.Guild, channel, designation);

        [Command("add")]
        [Summary("Assigns a designation to the current channel")]
        public Task AddLogChannel(
            [Summary("The designation to assign")]
                ChannelDesignation designation)
            => DesignatedChannelService.AddDesignatedChannelAsync(Context.Guild, Context.Channel, designation);

        [Command("remove")]
        [Summary("Removes a designation from the given channel")]
        public Task RemoveLogChannel(
            [Summary("The channel to unassign")]
                IMessageChannel channel,
            [Summary("The designation to unassign from")]
                ChannelDesignation designation)
            => DesignatedChannelService.RemoveDesignatedChannelAsync(Context.Guild, channel, designation);

        [Command("remove")]
        [Summary("Removes a designation from the current channel")]
        public Task RemoveLogChannel(
            [Summary("The designation to unassign from")]
                ChannelDesignation designation)
            => DesignatedChannelService.RemoveDesignatedChannelAsync(Context.Guild, Context.Channel, designation);
    }
}
