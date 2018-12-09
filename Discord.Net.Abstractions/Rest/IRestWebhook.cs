using System;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestWebhook" />
    public interface IRestWebhook : IWebhook, IUpdateable { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="RestWebhook"/>, through the <see cref="IRestWebhook"/> interface.
    /// </summary>
    public class RestWebhookAbstraction : IRestWebhook
    {
        /// <summary>
        /// Constructs a new <see cref="RestWebhookAbstraction"/> around an existing <see cref="Rest.RestWebhook"/>.
        /// </summary>
        /// <param name="restWebhook">The value to use for <see cref="Rest.RestWebhook"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restWebhook"/>.</exception>
        public RestWebhookAbstraction(RestWebhook restWebhook)
        {
            if (restWebhook is null)
                throw new ArgumentNullException(nameof(restWebhook));

            RestWebhook = restWebhook;
        }

        /// <inheritdoc />
        public string AvatarId
            => RestWebhook.AvatarId;

        /// <inheritdoc />
        public ITextChannel Channel
            => (RestWebhook as IWebhook).Channel;

        /// <inheritdoc />
        public ulong ChannelId
            => RestWebhook.ChannelId;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => RestWebhook.CreatedAt;

        /// <inheritdoc />
        public IUser Creator
            => RestWebhook.Creator;

        /// <inheritdoc />
        public IGuild Guild
            => (RestWebhook as IWebhook).Guild;

        /// <inheritdoc />
        public ulong? GuildId
            => RestWebhook.GuildId;

        /// <inheritdoc />
        public ulong Id
            => RestWebhook.Id;

        /// <inheritdoc />
        public string Name
            => RestWebhook.Name;

        /// <inheritdoc />
        public string Token
            => RestWebhook.Token;

        /// <inheritdoc />
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => RestWebhook.GetAvatarUrl(format, size);

        /// <inheritdoc />
        public Task ModifyAsync(Action<WebhookProperties> func, RequestOptions options = null)
            => RestWebhook.ModifyAsync(func, options);

        /// <inheritdoc />
        public Task DeleteAsync(RequestOptions options = null)
            => RestWebhook.DeleteAsync(options);

        /// <inheritdoc />
        public Task UpdateAsync(RequestOptions options = null)
            => RestWebhook.UpdateAsync(options);

        /// <inheritdoc cref="RestWebhook.ToString" />
        public override string ToString()
            => RestWebhook.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestWebhook"/> being abstracted.
        /// </summary>
        protected RestWebhook RestWebhook { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestWebhook"/> objects.
    /// </summary>
    public static class RestWebhookAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestWebhook"/> to an abstracted <see cref="IRestWebhook"/> value.
        /// </summary>
        /// <param name="restWebhook">The existing <see cref="RestWebhook"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restWebhook"/>.</exception>
        /// <returns>An <see cref="IRestWebhook"/> that abstracts <paramref name="restWebhook"/>.</returns>
        public static IRestWebhook Abstract(this RestWebhook restWebhook)
            => new RestWebhookAbstraction(restWebhook);
    }
}
