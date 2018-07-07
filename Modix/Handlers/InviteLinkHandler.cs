using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Modix.Services.Configuration;
using Serilog;

namespace Modix.Handlers
{
    public class InviteLinkHandler
    {
        private readonly DiscordBotConfiguration _botConfiguration;

        private const string InviteLinkPattern = @"(https?:\/\/)?(www\.)?(discord\.(gg|io|me|li)|discordapp\.com\/invite)\/.+[a-z]";

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

            var matches = GetRegexCheck(InviteLinkPattern).Matches(message.Content);

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
                var loggingChannel = originalChannel.Guild.Channels.SingleOrDefault(x => x.Id == _botConfiguration.InvitePurging.LoggingChannelId) as SocketTextChannel;

                if (loggingChannel == null)
                {
                    Log.Debug("logging channel with ID: {id} not found", _botConfiguration.InvitePurging.LoggingChannelId);
                    return;
                }

                await SendToLoggingChannel(loggingChannel, message);
            }
            catch (Exception e)
            {
                Log.Debug(e, "Failed posting to logging channel {channelId}", _botConfiguration.InvitePurging.LoggingChannelId);
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

        private static Regex GetRegexCheck(string pattern)
            => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));
    }
}
