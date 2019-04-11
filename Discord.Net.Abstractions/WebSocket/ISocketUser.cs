using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketUser" />
    public interface ISocketUser : IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence
    {
        /// <inheritdoc cref="SocketUser.MutualGuilds" />
        IReadOnlyCollection<ISocketGuild> MutualGuilds { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketUser"/>, through the <see cref="ISocketUser"/> interface.
    /// </summary>
    internal class SocketUserAbstraction : ISocketUser
    {
        /// <summary>
        /// Constructs a new <see cref="SocketUserAbstraction"/> around an existing <see cref="WebSocket.SocketUser"/>.
        /// </summary>
        /// <param name="socketUser">The value to use for <see cref="WebSocket.SocketUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketUser"/>.</exception>
        public SocketUserAbstraction(SocketUser socketUser)
        {
            SocketUser = socketUser ?? throw new ArgumentNullException(nameof(socketUser));
        }

        /// <inheritdoc />
        public ulong Id
            => SocketUser.Id;

        /// <inheritdoc />
        public bool IsBot
            => SocketUser.IsBot;

        /// <inheritdoc />
        public string Username
            => SocketUser.Username;

        /// <inheritdoc />
        public ushort DiscriminatorValue
            => SocketUser.DiscriminatorValue;

        /// <inheritdoc />
        public string AvatarId
            => SocketUser.AvatarId;

        /// <inheritdoc />
        public bool IsWebhook
            => SocketUser.IsWebhook;

        /// <inheritdoc />
        public DateTimeOffset CreatedAt
            => SocketUser.CreatedAt;

        /// <inheritdoc />
        public string Discriminator
            => SocketUser.Discriminator;

        /// <inheritdoc />
        public string Mention
            => SocketUser.Mention;

        /// <inheritdoc />
        public IActivity Activity
            => SocketUser.Activity;

        /// <inheritdoc />
        public UserStatus Status
            => SocketUser.Status;

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketGuild> MutualGuilds
            => SocketUser.MutualGuilds
                .Select(x => x.Abstract())
                .ToArray();

        /// <inheritdoc />
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => SocketUser.GetAvatarUrl(format, size);

        /// <inheritdoc />
        public string GetDefaultAvatarUrl()
            => SocketUser.GetDefaultAvatarUrl();

        /// <inheritdoc />
        public async Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
            => (await SocketUser.GetOrCreateDMChannelAsync(options))
                .Abstract();

        /// <inheritdoc cref="SocketUser.ToString" />
        public override string ToString()
            => SocketUser.ToString();

        /// <summary>
        /// The existing <see cref="WebSocket.SocketUser"/> being abstracted.
        /// </summary>
        protected SocketUser SocketUser { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketUser"/> objects.
    /// </summary>
    internal static class SocketUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketUser"/> to an abstracted <see cref="ISocketUser"/> value.
        /// </summary>
        /// <param name="socketUser">The existing <see cref="SocketUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketUser"/>.</exception>
        /// <returns>An <see cref="ISocketUser"/> that abstracts <paramref name="socketUser"/>.</returns>
        public static ISocketUser Abstract(this SocketUser socketUser)
            => socketUser switch
            {
                null
                    => throw new ArgumentNullException(nameof(socketUser)),
                SocketGroupUser socketGroupUser
                    => SocketGroupUserAbstractionExtensions.Abstract(socketGroupUser) as ISocketUser,
                SocketGuildUser socketGuildUser
                    => SocketGuildUserAbstractionExtensions.Abstract(socketGuildUser) as ISocketUser,
                SocketSelfUser socketSelfUser
                    => SocketSelfUserAbstractionExtensions.Abstract(socketSelfUser) as ISocketUser,
                SocketUnknownUser socketUnknownUser
                    => SocketUnknownUserAbstractionExtensions.Abstract(socketUnknownUser) as ISocketUser,
                SocketWebhookUser socketWebhookUser
                    => SocketWebhookUserAbstractionExtensions.Abstract(socketWebhookUser) as ISocketUser,
                _
                    => new SocketUserAbstraction(socketUser) as ISocketUser // for internal type SocketGlobalUser
            };
    }
}
