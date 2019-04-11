using System;

namespace Discord
{
    /// <inheritdoc cref="MessageApplication" />
    public interface IMessageApplication
    {
        /// <inheritdoc cref="MessageApplication.Id" />
        ulong Id { get; }

        /// <inheritdoc cref="MessageApplication.CoverImage" />
        string CoverImage { get; }

        /// <inheritdoc cref="MessageApplication.Description" />
        string Description { get; }

        /// <inheritdoc cref="MessageApplication.Icon" />
        string Icon { get; }

        /// <inheritdoc cref="MessageApplication.IconUrl" />
        string IconUrl { get; }

        /// <inheritdoc cref="MessageApplication.Name" />
        string Name { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Discord.MessageApplication"/>, through the <see cref="IMessageApplication"/> interface.
    /// </summary>
    internal class MessageApplicationAbstraction : IMessageApplication
    {
        /// <summary>
        /// Constructs a new <see cref="MessageApplicationAbstraction"/> around an existing <see cref="Discord.MessageApplication"/>.
        /// </summary>
        /// <param name="messageApplication">The existing <see cref="Discord.MessageApplication"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="messageApplication"/>.</exception>
        public MessageApplicationAbstraction(MessageApplication messageApplication)
        {
            MessageApplication = messageApplication ?? throw new ArgumentNullException(nameof(messageApplication));
        }

        /// <inheritdoc />
        public ulong Id
            => MessageApplication.Id;

        /// <inheritdoc />
        public string CoverImage
            => MessageApplication.CoverImage;

        /// <inheritdoc />
        public string Description
            => MessageApplication.Description;

        /// <inheritdoc />
        public string Icon
            => MessageApplication.Icon;

        /// <inheritdoc />
        public string IconUrl
            => MessageApplication.IconUrl;

        /// <inheritdoc />
        public string Name
            => MessageApplication.Name;

        /// <inheritdoc cref="MessageApplication.ToString" />
        public override string ToString()
            => MessageApplication.ToString();

        /// <summary>
        /// The existing <see cref="Discord.MessageApplication"/> being abstracted.
        /// </summary>
        protected MessageApplication MessageApplication { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="MessageApplication"/> objects.
    /// </summary>
    public static class MessageApplicationAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="MessageApplication"/> to an abstracted <see cref="IMessageApplication"/> value.
        /// </summary>
        /// <param name="messageApplication">The existing <see cref="MessageApplication"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="messageApplication"/>.</exception>
        /// <returns>An <see cref="IMessageApplication"/> that abstracts <paramref name="messageApplication"/>.</returns>
        public static IMessageApplication Abstract(this MessageApplication messageApplication)
            => new MessageApplicationAbstraction(messageApplication);
    }
}
