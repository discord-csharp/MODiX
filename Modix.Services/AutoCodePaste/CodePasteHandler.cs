using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Modix.Services.Messages.Discord;

namespace Modix.Services.AutoCodePaste
{
    internal enum ReactionState
    {
        Added,
        Removed
    }

    /// <summary>
    /// Allows authorized users to react to a message with a tl;dr emote to re-upload a message
    /// to a code sharing service.
    /// </summary>
    public class CodePasteHandler :
        INotificationHandler<ReactionAdded>,
        INotificationHandler<ReactionRemoved>
    {
        public CodePasteHandler(CodePasteService service)
        {
            _service = service;
        }

        private readonly Dictionary<ulong, int> _repasteRatings = new Dictionary<ulong, int>();
        private readonly CodePasteService _service;

        private async Task ModifyRatings(Cacheable<IUserMessage, ulong> cachedMessage, SocketReaction reaction, ReactionState state)
        {
            if (reaction.Emote.Name != "tldr")
            {
                return;
            }

            var message = await cachedMessage.GetOrDownloadAsync();

            if (message.Content.Length < 100)
            {
                return;
            }

            var roles = (reaction.User.GetValueOrDefault() as SocketGuildUser)?.Roles;

            if (roles == null)
            {
                return;
            }

            _repasteRatings.TryGetValue(message.Id, out var currentRating);

            var modifier = state == ReactionState.Added ? 1 : -1;

            if (roles.Count > 1)
            {
                currentRating += 2 * modifier;
            }
            else
            {
                currentRating += 1 * modifier;
            }

            _repasteRatings[message.Id] = currentRating;

            if (currentRating >= 2)
            {
                await UploadMessage(message);
                _repasteRatings.Remove(message.Id);
            }
        }

        public Task Handle(ReactionAdded notification, CancellationToken cancellationToken)
            => ModifyRatings(notification.Message, notification.Reaction, ReactionState.Added);

        public Task Handle(ReactionRemoved notification, CancellationToken cancellationToken)
            => ModifyRatings(notification.Message, notification.Reaction, ReactionState.Removed);

        private async Task UploadMessage(IUserMessage arg)
        {
            try
            {
                var url = await _service.UploadCodeAsync(arg);
                var embed = _service.BuildEmbed(arg.Author, arg.Content, url);

                await arg.Channel.SendMessageAsync(arg.Author.Mention, false, embed);
                await arg.DeleteAsync();
            }
            catch (WebException ex)
            {
                await arg.Channel.SendMessageAsync($"I would have reuploaded your long message, but: {ex.Message}");
            }
        }
    }
}
