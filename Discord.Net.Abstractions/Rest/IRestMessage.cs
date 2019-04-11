using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestMessage" />
    public interface IRestMessage : IMessage, IUpdateable
    {
        /// <inheritdoc cref="RestMessage.MentionedUsers" />
        IReadOnlyCollection<IRestUser> MentionedUsers { get; }

        /// <inheritdoc cref="RestMessage.Embeds" />
        new IReadOnlyCollection<IEmbed> Embeds { get; }

        /// <inheritdoc cref="RestMessage.Attachments" />
        new IReadOnlyCollection<IAttachment> Attachments { get; }

        /// <inheritdoc cref="RestMessage.Application" />
        new IMessageApplication Application { get; }

        /// <inheritdoc cref="RestMessage.Activity" />
        new IMessageActivity Activity { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestMessage"/>, through the <see cref="IRestMessage"/> interface.
    /// </summary>
    internal abstract class RestMessageAbstraction : IRestMessage
    {
        /// <summary>
        /// Constructs a new <see cref="RestMessageAbstraction"/> around an existing <see cref="Rest.RestMessage"/>.
        /// </summary>
        /// <param name="restMessage">The value to use for <see cref="Rest.RestMessage"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restMessage"/>.</exception>
        protected RestMessageAbstraction(RestMessage restMessage)
        {
            RestMessage = restMessage ?? throw new ArgumentNullException(nameof(restMessage));
        }

        /// <inheritdoc />
        public IMessageActivity Activity
            => RestMessage.Activity
                .Abstract();

        /// <inheritdoc />
        MessageActivity IMessage.Activity
            => RestMessage.Activity;

        /// <inheritdoc />
        public IMessageApplication Application
            => RestMessage.Application
                .Abstract();

        /// <inheritdoc />
        MessageApplication IMessage.Application
            => RestMessage.Application;

        /// <inheritdoc />
        public IReadOnlyCollection<IAttachment> Attachments
            => RestMessage.Attachments;

        /// <inheritdoc />
        public IUser Author
            => RestMessage.Author
                .Abstract();

        /// <inheritdoc />
        public IMessageChannel Channel
            => RestMessage.Channel
                .Abstract();

        /// <inheritdoc />
        public string Content
            => RestMessage.Content;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => RestMessage.CreatedAt;

        /// <inheritdoc />
        public DateTimeOffset? EditedTimestamp
            => RestMessage.EditedTimestamp;

        /// <inheritdoc />
        public IReadOnlyCollection<IEmbed> Embeds
            => RestMessage.Embeds;

        /// <inheritdoc />
        public ulong Id
            => RestMessage.Id;

        /// <inheritdoc />
        public bool IsPinned
            => RestMessage.IsPinned;

        /// <inheritdoc />
        public bool IsTTS
            => RestMessage.IsTTS;

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> MentionedChannelIds
            => RestMessage.MentionedChannelIds;

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> MentionedRoleIds
            => RestMessage.MentionedRoleIds;

        /// <inheritdoc />
        public IReadOnlyCollection<ulong> MentionedUserIds
            => (RestMessage as IMessage).MentionedUserIds;

        /// <inheritdoc />
        public IReadOnlyCollection<IRestUser> MentionedUsers
            => RestMessage.MentionedUsers
                .Select(RestUserAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public MessageSource Source
            => RestMessage.Source;

        /// <inheritdoc />
        public IReadOnlyCollection<ITag> Tags
            => RestMessage.Tags;

        /// <inheritdoc />
        public DateTimeOffset Timestamp
            => RestMessage.Timestamp;

        /// <inheritdoc />
        public MessageType Type
            => (RestMessage as IMessage).Type;

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => RestMessage.DeleteAsync(options);

        /// <inheritdoc />
        public Task UpdateAsync(RequestOptions options = null)
            => RestMessage.UpdateAsync(options);

        /// <inheritdoc cref="RestMessage.ToString" />
        public override string ToString()
            => RestMessage.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestMessage"/> being abstracted.
        /// </summary>
        protected RestMessage RestMessage { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestMessage"/> objects.
    /// </summary>
    internal static class RestMessageAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestMessage"/> to an abstracted <see cref="IRestMessage"/> value.
        /// </summary>
        /// <param name="restMessage">The existing <see cref="RestMessage"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restMessage"/>.</exception>
        /// <returns>An <see cref="IRestMessage"/> that abstracts <paramref name="restMessage"/>.</returns>
        public static IRestMessage Abstract(this RestMessage restMessage)
            => restMessage switch
            {
                null
                    => throw new ArgumentNullException(nameof(restMessage)),
                RestSystemMessage restSystemMessage
                    => restSystemMessage.Abstract() as IRestMessage,
                RestUserMessage restUserMessage
                    => restUserMessage.Abstract() as IRestMessage,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(RestMessage)} type {restMessage.GetType().Name}")
            };
    }
}
