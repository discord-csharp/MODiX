using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Modix.Data;
using Modix.Data.Models.Core;
using Modix.Data.Models.Moderation;
using Modix.Data.Repositories;
using Modix.RemoraShim.Services;
using Modix.Services.Core;
using Modix.Services.MessageContentPatterns;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Serilog;

namespace Modix.RemoraShim.Responders
{
    public class MessageCheckResponder : IResponder<IMessageCreate>, IResponder<IMessageUpdate>
    {
        private readonly IMessageContentPatternService _msgContentPatternSvc;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly IDesignatedChannelService _designatedChannelService;
        private readonly IAuthorizationService _authService;
        private readonly IAuthorizationContextService _remoraAuthService;
        private readonly IDeletedMessageRepository _deletedMessageRepository;
        private readonly IThreadService _threadSvc;

        public MessageCheckResponder(
            IMessageContentPatternService msgContentPatternSvc,
            IDiscordRestChannelAPI channelApi,
            IDiscordRestUserAPI userApi,
            IDesignatedChannelService designatedChannelService,
            IAuthorizationService authService,
            IAuthorizationContextService remoraAuthService,
            IDeletedMessageRepository deletedRepository,
            IThreadService threadService
            )
        {
            _msgContentPatternSvc = msgContentPatternSvc;
            _channelApi = channelApi;
            _userApi = userApi;
            _designatedChannelService = designatedChannelService;
            _authService = authService;
            _remoraAuthService = remoraAuthService;
            _deletedMessageRepository = deletedRepository;
            _threadSvc = threadService;
        }

        public async Task<Result> RespondAsync(IMessageCreate message, CancellationToken ct = default)
        {
            var msg = message.ToPartialMessage();
            await TryCheckMessageAsync(msg, ct);
            return Result.FromSuccess();
        }

        public async Task<Result> RespondAsync(IMessageUpdate message, CancellationToken ct = default)
        {
            await TryCheckMessageAsync(message, ct);
            return Result.FromSuccess();
        }

        private async Task TryCheckMessageAsync(IPartialMessage message, CancellationToken ct = default)
        {
            var channelId = message.ChannelID.Value;

            var isThreadChannel = await _threadSvc.IsThreadChannelAsync(channelId, ct);
            if (!isThreadChannel)
            {
                return;
            }

            if (!message.GuildID.HasValue)
            {
                return;
            }

            if (!message.Author.HasValue)
            {
                return;
            }

            var author = message.Author.Value;
            var authorId = author.ID;
            var messageId = message.ID.Value;
            var guildId = message.GuildID.Value;

            if ((author.IsBot.HasValue && author.IsBot.Value)
                || (author.IsSystem.HasValue && author.IsSystem.Value))
            {
                return;
            }

            if (!message.Content.HasValue)
            {
                return;
            }

            var isContentBlocked = await IsContentBlockedAsync(message);

            if (!isContentBlocked)
            {
                return;
            }

            if (await _designatedChannelService.ChannelHasDesignationAsync(guildId.Value, channelId.Value, DesignatedChannelType.Unmoderated, default))
            {
                return;
            }

            var roles = message.Member.Value.Roles.Value.Select(a => a.Value).ToList();
            roles.Add(guildId.Value);

            await _remoraAuthService.SetCurrentAuthenticatedUserAsync(guildId, authorId);
            if (await _authService.HasClaimsAsync(authorId.Value, messageId.Value, roles, AuthorizationClaim.BypassMessageContentPatternCheck))
            {
                Log.Debug("Message {MessageId} was skipped because the author {Author} has the {Claim} claim",
                    messageId, authorId, AuthorizationClaim.BypassMessageContentPatternCheck);
                return;
            }
            Log.Debug("Message {MessageId} is going to be deleted", message.ID);

            using var transaction = await _deletedMessageRepository.BeginCreateTransactionAsync(ct);

            var reason = "Unauthorized Message Content";
            await _channelApi.DeleteMessageAsync(channelId, messageId, reason, ct);
            var self = await _userApi.GetCurrentUserAsync(ct);
            await _deletedMessageRepository.CreateAsync(
                new DeletedMessageCreationData()
                {
                    GuildId = guildId.Value,
                    ChannelId = channelId.Value,
                    MessageId = messageId.Value,
                    AuthorId = authorId.Value,
                    Content = message.Content.Value,
                    Reason = reason,
                    CreatedById = self.Entity!.ID.Value
                }, ct);

            transaction.Commit();

            Log.Debug("Message {MessageId} was deleted because it contains blocked content", message.ID);

            await _channelApi.CreateMessageAsync(channelId, $"Sorry <@{author.ID.Value}> your message contained blocked content and has been removed!", ct: ct);
        }


        private async Task<bool> IsContentBlockedAsync(IPartialMessage message)
        {
            var patterns = await _msgContentPatternSvc.GetPatterns(message.GuildID.Value.Value);

            foreach (var patternToCheck in patterns.Where(x => x.Type == MessageContentPatternType.Blocked))
            {
                // If the content is not blocked, we can just continue to check the next
                // blocked pattern

                var (containsBlockedPattern, blockedMatches) = GetContentMatches(message.Content.Value, patternToCheck);

                if (!containsBlockedPattern)
                {
                    continue;
                }

                var allowedPatterns = patterns.Where(x => x.Type == MessageContentPatternType.Allowed).ToList();

                if (!allowedPatterns.Any())
                {
                    return true;
                }

                foreach (Match blockedMatch in blockedMatches)
                {
                    var didFindAllowedPattern = false;

                    foreach (var allowPattern in allowedPatterns)
                    {
                        var (hasAllowedMatch, _) = GetContentMatches(blockedMatch.Value, allowPattern);
                        didFindAllowedPattern = hasAllowedMatch;
                    }

                    if (!didFindAllowedPattern)
                        return true;
                }

                return false;

                static (bool, MatchCollection) GetContentMatches(string content, MessageContentPatternDto pattern)
                {
                    var matches = pattern.Regex.Matches(content);
                    return (matches.Any(), matches);
                }
            }

            return false;
        }
    }
}
