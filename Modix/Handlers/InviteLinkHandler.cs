using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.BehaviourConfiguration;
using Serilog;

namespace Modix.Handlers
{
    public class InviteLinkHandler
    {
        private readonly IBehaviourConfiguration _botConfiguration;

        private static readonly Regex InviteLinkPattern = new Regex(@"(https?:\/\/)?(www\.)?(discord\.(gg|io|me|li)|discordapp\.com\/invite)\/.+[a-z]", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));

        public InviteLinkHandler(IBehaviourConfiguration botConfiguration)
        {
            _botConfiguration = botConfiguration;
        }

        /// <summary>
        /// Will scan a message for any matchable Discord invite links
        /// that are not part of the context's current Guild.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>False if no Invite link was found, true if an invite link was found and the message was purged</returns>
        public async Task<bool> PurgeInviteLink(IMessage message)
        {
            if (!(message.Channel is SocketGuildChannel channel))
                return false;

            if (!(message.Author is SocketGuildUser user))
                return false;

            if (!_botConfiguration.InvitePurgeBehaviour.IsEnabled)
                return false;

            var matches = InviteLinkPattern.Matches(message.Content);

            var invites = await channel.GetInvitesAsync();

            var guildsInvites = invites.Select(x => x.Url);

            var attemptedInvites = matches.Select(x => x.Value);

            if (!attemptedInvites.Except(guildsInvites).Any())
                return false;

            Log.Debug("Found matching invite link present in message {messageId}", message.Id);

            var exemptIds = _botConfiguration.InvitePurgeBehaviour.ExemptRoleIds;

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
                var loggingChannel = originalChannel.Guild.Channels.SingleOrDefault(x => x.Id == _botConfiguration.InvitePurgeBehaviour.LoggingChannelId) as SocketTextChannel;

                if (loggingChannel == null)
                {
                    Log.Debug("logging channel with ID: {id} not found", _botConfiguration.InvitePurgeBehaviour.LoggingChannelId);
                    return;
                }

                await SendToLoggingChannel(loggingChannel, message);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed posting to logging channel {channelId}", _botConfiguration.InvitePurgeBehaviour.LoggingChannelId);
            }
        }

        private async Task TryNotifyUser(ISocketMessageChannel originalChannel, IMessage message)
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

        private static Task SendToLoggingChannel(ISocketMessageChannel loggingChannel, IMessage originalMessage)
        {
            var header = $"🚧 Invite purged - Originally posted by {originalMessage.Author.Mention} `({originalMessage.Author.Id})` at `{originalMessage.Timestamp:dd/MM/yyyy HH:mm:ss}` in #{originalMessage.Channel.Name} `({originalMessage.Channel.Id})`";

            var formattedContent = "\n```" + originalMessage.Content + "```";

            return loggingChannel.SendMessageAsync(header + formattedContent);
        }
    }
}
