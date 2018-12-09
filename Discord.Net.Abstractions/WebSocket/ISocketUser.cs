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
    public class SocketUserAbstraction : ISocketUser
    {
        /// <summary>
        /// Constructs a new <see cref="SocketUserAbstraction"/> around an existing <see cref="WebSocket.SocketUser"/>.
        /// </summary>
        /// <param name="socketUser">The value to use for <see cref="WebSocket.SocketUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketUser"/>.</exception>
        public SocketUserAbstraction(SocketUser socketUser)
        {
            if (socketUser is null)
                throw new ArgumentNullException(nameof(socketUser));

            MutualGuilds = socketUser.MutualGuilds
                .Select(x => x.Abstract())
                .ToArray();
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
        public IReadOnlyCollection<ISocketGuild> MutualGuilds { get; }

        /// <inheritdoc />
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => SocketUser.GetAvatarUrl(format, size);

        /// <inheritdoc />
        public string GetDefaultAvatarUrl()
            => SocketUser.GetDefaultAvatarUrl();

        /// <inheritdoc />
        public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
            => SocketUser.GetOrCreateDMChannelAsync(options);

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
    public static class SocketUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketUser"/> to an abstracted <see cref="ISocketUser"/> value.
        /// </summary>
        /// <param name="socketUser">The existing <see cref="SocketUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketUser"/>.</exception>
        /// <returns>An <see cref="ISocketUser"/> that abstracts <paramref name="socketUser"/>.</returns>
        public static ISocketUser Abstract(this SocketUser socketUser)
            => (socketUser is null) ? throw new ArgumentNullException(nameof(socketUser))
                : (socketUser is SocketGroupUser socketGroupUser) ? socketGroupUser.Abstract() as ISocketUser
                : (socketUser is SocketGuildUser socketGuildUser) ? socketGuildUser.Abstract() as ISocketUser
                : (socketUser is SocketSelfUser socketSelfUser) ? socketSelfUser.Abstract() as ISocketUser
                : (socketUser is SocketUnknownUser socketUnknownUser) ? socketUnknownUser.Abstract() as ISocketUser
                : (socketUser is SocketWebhookUser socketWebhookUser) ? socketWebhookUser.Abstract() as ISocketUser
                : new SocketUserAbstraction(socketUser) as ISocketUser; // for internal type SocketGuildUser
    }
}
