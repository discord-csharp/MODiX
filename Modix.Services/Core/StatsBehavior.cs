using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Modix.Data.Models.Core;
using StatsdClient;

namespace Modix.Services.Core
{
    public class StatsBehavior : BehaviorBase
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly IDogStatsd _stats;
        private readonly ILogger<StatsBehavior> _log;

        public StatsBehavior(
            DiscordSocketClient discordClient,
            IDogStatsd stats,
            ILogger<StatsBehavior> log,
            IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _discordClient = discordClient;
            _stats = stats;
            _log = log;
        }

        protected internal override Task OnStartingAsync()
        {
            _discordClient.MessageReceived += HandleMessageReceived;

            return Task.CompletedTask;
        }

        protected internal override Task OnStoppedAsync()
        {
            _discordClient.MessageReceived -= HandleMessageReceived;

            return Task.CompletedTask;
        }

        private async Task HandleMessageReceived(IMessage message)
        {
            if (message.Channel is IGuildChannel channel &&
                message.Author is IGuildUser author &&
                author.Guild is IGuild guild &&
                !author.IsBot && !author.IsWebhook)
            {
                var tags = new List<string>();

                tags.Add("guild:" + guild.Name);
                tags.Add("channel:" + channel.Name);

                var isParticipation = true;
                try
                {
                    await SelfExecuteRequest<IDesignatedChannelService>(async d
                        => isParticipation = await d.ChannelHasDesignationAsync(guild, channel,
                            DesignatedChannelType.CountsTowardsParticipation));
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Unable to check if channel counts towards participation.");
                }

                tags.Add("offtopic:" + !isParticipation);

                var isRankedUser = author.RoleIds.Count > 1;

                tags.Add("has_role:" + isRankedUser);


                _stats.Increment("messages_received", tags: tags.ToArray());
            }
        }
    }
}
