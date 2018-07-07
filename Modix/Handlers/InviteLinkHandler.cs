using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Common;
using Modix.Services.Configuration;
using Serilog;

namespace Modix.Handlers
{
    public class InviteLinkHandler
    {
        private readonly DiscordBotConfiguration _botConfiguration;

        public InviteLinkHandler(DiscordBotConfiguration botConfiguration)
        {
            _botConfiguration = botConfiguration;
        }

        public async Task<bool> PurgeInviteLink(IMessage message)
        {
            if (!(message.Channel is SocketGuildChannel channel))
                return false;

            if (!(message.Author is SocketGuildUser user))
                return false;

            if (!_botConfiguration.PurgeInvites)
                return false;

            var inviteRegex = _botConfiguration.InvitePurging.ValidatingRegex;

            var matches = inviteRegex.CheckMatch().Matches(message.Content);

            var invites = await channel.GetInvitesAsync();

            var guildsInvites = invites.Select(x => x.Url);

            var attemptedInvites = matches.Select(x => x.Value);

            if (!attemptedInvites.Except(guildsInvites).Any())
                return false;

            Log.Debug("Found matching invite link present in message {messageId}", message.Id);

            var exemptIds = _botConfiguration.InvitePurging.ExemptRoles;

            if (user.Roles.Select(x => x.Id).Any(x => exemptIds.Contains(x)))
                return false;

            var originalChannel = (SocketTextChannel)message.Channel;

            await TryPostToDebugChannel(originalChannel, message);

            await message.DeleteAsync();

            await TryNotifyUser(originalChannel, message);

            return true;
        }

        private async Task TryPostToDebugChannel(SocketGuildChannel originalChannel, IMessage message)
        {
            try
            {
                var moderationChannel = originalChannel.Guild.Channels.SingleOrDefault(x => x.Id == _botConfiguration.InvitePurging.LoggingChannelId) as SocketTextChannel;

                if (moderationChannel == null)
                {
                    Log.Debug("Moderation channel with ID: {id} not found", _botConfiguration.InvitePurging.LoggingChannelId);
                    return;
                }

                await SendToLoggingChannel(message);
            }
            catch (Exception e)
            {
                Log.Debug(e, "Failed posting to moderation channel {channelId}", _botConfiguration.InvitePurging.LoggingChannelId);
            }
        }

        private async Task TryNotifyUser(SocketTextChannel originalChannel, IMessage message)
        {
            try
            {
                await originalChannel.SendMessageAsync($"{message.Author.Mention} your invite link has been removed, please don't post links to other Guilds");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed responding to user's purged invite link in channel {channelId}", originalChannel.Id);
            }
        }

        private static Task SendToLoggingChannel(IMessage originalMessage)
        {
            var header = $"🚧 Invite purged - Originally posted by {originalMessage.Author.Username} ({originalMessage.Author.Id}) at `{originalMessage.Timestamp:dd/MM/yyyy HH:mm:ss}`";

            var formattedContent = "\n\n```" + originalMessage.Content + "```";

            return originalMessage.Channel.SendMessageAsync(header + formattedContent);
        }
    }
}
