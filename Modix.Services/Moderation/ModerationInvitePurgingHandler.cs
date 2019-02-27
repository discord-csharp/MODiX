using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using MediatR;
using Modix.Data.Models.Core;
using Modix.Services.Core;
using Modix.Services.Messages.Discord;
using Serilog;

namespace Modix.Services.Moderation
{
    /// <summary>
    /// Implements a handler that automatically deletes invite links posted by select users.
    /// </summary>
    public class ModerationInvitePurgingHandler :
        INotificationHandler<ChatMessageReceived>,
        INotificationHandler<ChatMessageUpdated>
    {
        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IModerationService _moderationService;
        private readonly ISelfUser _botUser;

        /// <summary>
        /// Constructs a new <see cref="ModerationInvitePurgingHandler"/> object, with the given injected dependencies.
        /// </summary>
        /// <param name="designatedChannelService"></param>
        /// <param name="authorizationService"></param>
        /// <param name="moderationService"></param>
        /// <param name="botUser">The Discord user that the bot is running as.</param>
        public ModerationInvitePurgingHandler(
            IDesignatedChannelService designatedChannelService,
            IAuthorizationService authorizationService,
            IModerationService moderationService,
            ISelfUser botUser)
        {
            _designatedChannelService = designatedChannelService;
            _authorizationService = authorizationService;
            _moderationService = moderationService;
            _botUser = botUser;
        }

        public Task Handle(ChatMessageReceived notification, CancellationToken cancellationToken) =>
            TryPurgeInviteLink(notification.Message);

        public Task Handle(ChatMessageUpdated notification, CancellationToken cancellationToken) =>
            TryPurgeInviteLink(notification.NewMessage);

        /// <summary>
        /// Determines whether or not to skip a message event, based on unmoderated channel designations
        /// </summary>
        /// <param name="guild">The guild designations should be looked up for</param>
        /// <param name="channel">The channel designations should be looked up for</param>
        /// <returns>True if the channel is designated as Unmoderated, false if not</returns>
        private async Task<bool> IsChannelModeratedAsync(IGuild guild, IMessageChannel channel)
        {
            return await _designatedChannelService.ChannelHasDesignationAsync(guild, channel, DesignatedChannelType.Unmoderated);
        }

        /// <summary>
        /// Deletes the message if it contains an invite link.
        /// </summary>
        /// <param name="message">The message to possibly purge.</param>
        /// <returns></returns>
        private async Task TryPurgeInviteLink(IMessage message)
        {
            if
            (
                !(message.Author is IGuildUser author) ||
                !(message.Channel is IGuildChannel guildChannel) ||
                !(message.Channel is IMessageChannel msgChannel)
            )
            {
                Log.Debug("Message {MessageId} was not in an IGuildChannel & IMessageChannel, or Author {Author} was not an IGuildUser",
                    message.Id, message.Author.Id);
                return;
            }

            if (author.Id == _botUser.Id)
            {
                return;
            }

            if (await IsChannelModeratedAsync(guildChannel.Guild, msgChannel))
            {
                return;
            }

            var matches = _inviteLinkMatcher.Matches(message.Content);
            if (!matches.Any())
            {
                return;
            }

            // TODO: Booooooo for non-abstractable dependencies
            if (author.Guild is SocketGuild socketGuild)
            {
                // Allow invites to the guild in which the message was posted
                var newInvites = matches
                    .Select(x => x.Value)
                    .Except((await socketGuild
                        .GetInvitesAsync())
                        .Select(x => x.Url));

                if (!newInvites.Any())
                {
                    Log.Debug("Message {MessageId} was skipped because the invite was to this server", message.Id);
                    return;
                }
            }

            if (await _authorizationService.HasClaimsAsync(author, AuthorizationClaim.PostInviteLink))
            {
                Log.Debug("Message {MessageId} was skipped because the author {Author} has the PostInviteLink claim",
                    message.Id, message.Author.Id);
                return;
            }

            Log.Debug("Message {MessageId} is going to be deleted", message.Id);

            await _moderationService.DeleteMessageAsync(message, "Unauthorized Invite Link");

            Log.Debug("Message {MessageId} was deleted because it contains an invite link", message.Id);

            await msgChannel.SendMessageAsync($"Sorry {author.Mention} your invite link has been removed - please don't post links to other guilds");
        }

        private static readonly Regex _inviteLinkMatcher
            = new Regex(
                pattern: @"(https?:\/\/)?(www\.)?(discord\.(gg|io|me|li)|discordapp\.com\/invite)\/.+[a-z]",
                options: RegexOptions.Compiled | RegexOptions.IgnoreCase,
                matchTimeout: TimeSpan.FromSeconds(2));


    }
}
