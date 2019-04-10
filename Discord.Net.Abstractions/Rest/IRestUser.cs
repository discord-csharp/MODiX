using System;
using System.Threading.Tasks;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestUser" />
    public interface IRestUser : IUser, IUpdateable
    {
        /// <inheritdoc cref="RestUser.GetOrCreateDMChannelAsync(RequestOptions)" />
        new Task<IRestDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestUser"/>, through the <see cref="IRestUser"/> interface.
    /// </summary>
    public class RestUserAbstraction : IRestUser
    {
        /// <summary>
        /// Constructs a new <see cref="RestUserAbstraction"/> around an existing <see cref="Rest.RestUser"/>.
        /// </summary>
        /// <param name="restUser">The value to use for <see cref="Rest.RestUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restUser"/>.</exception>
        public RestUserAbstraction(RestUser restUser)
        {
            RestUser = restUser ?? throw new ArgumentNullException(nameof(restUser));
        }

        /// <inheritdoc />
        public IActivity Activity
            => RestUser.Activity;

        /// <inheritdoc />
        public string AvatarId
            => RestUser.AvatarId;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => RestUser.CreatedAt;

        /// <inheritdoc />
        public string Discriminator
            => RestUser.Discriminator;

        /// <inheritdoc />
        public ushort DiscriminatorValue
            => RestUser.DiscriminatorValue;

        /// <inheritdoc />
        public ulong Id
            => RestUser.Id;

        /// <inheritdoc />
        public bool IsBot
            => RestUser.IsBot;

        /// <inheritdoc />
        public bool IsWebhook
            => RestUser.IsWebhook;

        /// <inheritdoc />
        public string Mention
            => RestUser.Mention;

        /// <inheritdoc />
        public UserStatus Status
            => RestUser.Status;

        /// <inheritdoc />
        public string Username
            => RestUser.Username;

        /// <inheritdoc />
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => RestUser.GetAvatarUrl(format, size);

        /// <inheritdoc />
        public string GetDefaultAvatarUrl()
            => RestUser.GetDefaultAvatarUrl();

        /// <inheritdoc />
        public async Task<IRestDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
            => (await RestUser.GetOrCreateDMChannelAsync(options))
                .Abstract();

        /// <inheritdoc />
        async Task<IDMChannel> IUser.GetOrCreateDMChannelAsync(RequestOptions options)
            => (await (RestUser as IUser).GetOrCreateDMChannelAsync(options))
                .Abstract();

        /// <inheritdoc />
        public Task UpdateAsync(RequestOptions options = null)
            => RestUser.UpdateAsync(options);

        /// <inheritdoc cref="RestUser.ToString"/>
        public override string ToString()
            => RestUser.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestUser"/> being abstracted.
        /// </summary>
        protected RestUser RestUser { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestUser"/> objects.
    /// </summary>
    public static class RestUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestUser"/> to an abstracted <see cref="IRestUser"/> value.
        /// </summary>
        /// <param name="restUser">The existing <see cref="RestUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restUser"/>.</exception>
        /// <returns>An <see cref="IRestUser"/> that abstracts <paramref name="restUser"/>.</returns>
        public static IRestUser Abstract(this RestUser restUser)
            => restUser switch
            {
                null
                    => throw new ArgumentNullException(nameof(restUser)),
                RestGroupUser restGroupUser
                    => restGroupUser.Abstract(),
                RestGuildUser restGuildUser
                    => restGuildUser.Abstract(),
                RestSelfUser restSelfUser
                    => restSelfUser.Abstract(),
                RestWebhookUser restWebhookUser
                    => restWebhookUser.Abstract(),
                _
                    => new RestUserAbstraction(restUser) as IRestUser
            };
    }
}
