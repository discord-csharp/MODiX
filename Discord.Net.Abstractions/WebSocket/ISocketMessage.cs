using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketMessage" />
    public interface ISocketMessage : IMessage, ISnowflakeEntity, IEntity<ulong>, IDeletable
    {
        /// <inheritdoc cref="SocketMessage.MentionedUsers" />
        IReadOnlyCollection<ISocketUser> MentionedUsers { get; }

        /// <inheritdoc cref="SocketMessage.MentionedRoles" />
        IReadOnlyCollection<ISocketRole> MentionedRoles { get; }

        /// <inheritdoc cref="SocketMessage.MentionedChannels" />
        IReadOnlyCollection<ISocketGuildChannel> MentionedChannels { get; }

        /// <inheritdoc cref="SocketMessage.Application" />
        new IMessageApplication Application { get; }

        /// <inheritdoc cref="SocketMessage.Activity" />
        new IMessageActivity Activity { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketMessage"/>, through the <see cref="ISocketMessage"/> interface.
    /// </summary>
    internal abstract class SocketMessageAbstraction : ISocketMessage
    {
        /// <summary>
        /// Constructs a new <see cref="SocketMessageAbstraction"/> around an existing <see cref="WebSocket.SocketMessage"/>.
        /// </summary>
        /// <param name="socketMessage">The value to use for <see cref="WebSocket.SocketMessage"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketMessage"/>.</exception>
        protected SocketMessageAbstraction(SocketMessage socketMessage)
        {
            SocketMessage = socketMessage ?? throw new ArgumentNullException(nameof(socketMessage));
        }

        /// <inheritdoc />
        public IMessageActivity Activity
            => SocketMessage.Activity
                .Abstract();

        /// <inheritdoc />
        MessageActivity IMessage.Activity
            => SocketMessage.Activity;

        /// <inheritdoc />
        public IMessageApplication Application
            => SocketMessage.Application
                .Abstract();

        /// <inheritdoc />
        public MessageReference Reference => SocketMessage.Reference;

        /// <inheritdoc />
        MessageApplication IMessage.Application
            => SocketMessage.Application;

        /// <inheritdoc />
        public IReadOnlyCollection<IAttachment> Attachments
            => SocketMessage.Attachments;

        /// <inheritdoc />
        public IUser Author
            => (SocketMessage as IMessage).Author
                .Abstract();

        /// <inheritdoc />
        public IMessageChannel Channel
            => (SocketMessage as IMessage).Channel
                .Abstract();

        public bool MentionedEveryone
            => SocketMessage.MentionedEveryone;

        /// <inheritdoc />
        public string Content
            => SocketMessage.Content;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => SocketMessage.CreatedAt;

        /// <inheritdoc />
        public DateTimeOffset? EditedTimestamp
            => SocketMessage.EditedTimestamp;

        /// <inheritdoc />
        public IReadOnlyCollection<IEmbed> Embeds
            => SocketMessage.Embeds;

        /// <inheritdoc />
        public ulong Id
            => SocketMessage.Id;

        /// <inheritdoc />
        public bool IsPinned
            => SocketMessage.IsPinned;

        /// <inheritdoc />
        public bool IsTTS
            => SocketMessage.IsTTS;

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> MentionedChannelIds
            => (SocketMessage as IMessage).MentionedChannelIds;

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketGuildChannel> MentionedChannels
            => SocketMessage.MentionedChannels
                .Select(SocketGuildChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> MentionedRoleIds
            => (SocketMessage as IMessage).MentionedRoleIds;

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketRole> MentionedRoles
            => SocketMessage.MentionedRoles
                .Select(SocketRoleAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> MentionedUserIds
            => (SocketMessage as IMessage).MentionedUserIds;

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketUser> MentionedUsers
            => SocketMessage.MentionedUsers
                .Select(SocketUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public MessageSource Source
            => SocketMessage.Source;

        /// <inheritdoc />
        public IReadOnlyCollection<ITag> Tags
            => SocketMessage.Tags;

        /// <inheritdoc />
        public DateTimeOffset Timestamp
            => SocketMessage.Timestamp;

        /// <inheritdoc />
        public MessageType Type
            => (SocketMessage as IMessage).Type;

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => SocketMessage.DeleteAsync(options);

        /// <inheritdoc cref="SocketMessage.ToString"/>
        public override string ToString()
            => SocketMessage.ToString();

        public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
            => SocketMessage.AddReactionAsync(emote, options);

        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
            => SocketMessage.RemoveReactionAsync(emote, user, options);

        public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions options = null)
            => SocketMessage.RemoveReactionAsync(emote, userId, options);

        public Task RemoveAllReactionsAsync(RequestOptions options = null)
            => SocketMessage.RemoveAllReactionsAsync(options);

        public Task RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions options = null)
            => SocketMessage.RemoveAllReactionsForEmoteAsync(emote, options);

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null)
            => SocketMessage.GetReactionUsersAsync(emoji, limit, options);

        /// <summary>
        /// The existing <see cref="WebSocket.SocketMessage"/> being abstracted.
        /// </summary>
        protected SocketMessage SocketMessage { get; }

        public bool IsSuppressed => SocketMessage.IsSuppressed;

        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => SocketMessage.Reactions;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketMessage"/> objects.
    /// </summary>
    internal static class SocketMessageAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketMessage"/> to an abstracted <see cref="ISocketMessage"/> value.
        /// </summary>
        /// <param name="socketMessage">The existing <see cref="SocketMessage"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketMessage"/>.</exception>
        /// <returns>An <see cref="ISocketMessage"/> that abstracts <paramref name="socketMessage"/>.</returns>
        public static ISocketMessage Abstract(this SocketMessage socketMessage)
            => socketMessage switch
            {
                null
                    => throw new ArgumentNullException(nameof(SocketMessage)),
                SocketUserMessage socketUserMessage
                    => SocketUserMessageAbstractionExtensions.Abstract(socketUserMessage) as ISocketMessage,
                SocketSystemMessage socketSystemMessage
                    => SocketSystemMessageAbstractionExtensions.Abstract(socketSystemMessage) as ISocketMessage,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(SocketMessage)} type {socketMessage.GetType().Name}")
            };
    }
}
