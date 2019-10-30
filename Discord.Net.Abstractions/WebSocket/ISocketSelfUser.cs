using System;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="SocketSelfUser" />
    public interface ISocketSelfUser : ISocketUser, ISelfUser { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="WebSocket.SocketSelfUser"/>, through the <see cref="ISocketSelfUser"/> interface.
    /// </summary>
    internal class SocketSelfUserAbstraction : SocketUserAbstraction, ISocketSelfUser
    {
        /// <summary>
        /// Constructs a new <see cref="SocketSelfUserAbstraction"/> around an existing <see cref="WebSocket.SocketSelfUser"/>.
        /// </summary>
        /// <param name="socketSelfUser">The value to use for <see cref="WebSocket.SocketSelfUser"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketSelfUser"/>.</exception>
        public SocketSelfUserAbstraction(SocketSelfUser socketSelfUser)
            : base(socketSelfUser) { }

        /// <inheritdoc />
        public string Email
            => SocketSelfUser.Email;

        /// <inheritdoc />
        public bool IsVerified
            => SocketSelfUser.IsVerified;

        /// <inheritdoc />
        public bool IsMfaEnabled
            => SocketSelfUser.IsMfaEnabled;

        /// <inheritdoc />
        public Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions options = null)
            => SocketSelfUser.ModifyAsync(func, options);

        /// <summary>
        /// The existing <see cref="WebSocket.SocketSelfUser"/> being abstracted.
        /// </summary>
        protected SocketSelfUser SocketSelfUser
            => SocketUser as SocketSelfUser;

        public UserProperties Flags => SocketSelfUser.Flags;

        public PremiumType PremiumType => SocketSelfUser.PremiumType;

        public string Locale => SocketSelfUser.Locale;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="SocketSelfUser"/> objects.
    /// </summary>
    internal static class SocketSelfUserAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="SocketSelfUser"/> to an abstracted <see cref="ISocketSelfUser"/> value.
        /// </summary>
        /// <param name="socketSelfUser">The existing <see cref="SocketSelfUser"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="socketSelfUser"/>.</exception>
        /// <returns>An <see cref="ISocketSelfUser"/> that abstracts <paramref name="socketSelfUser"/>.</returns>
        public static ISocketSelfUser Abstract(this SocketSelfUser socketSelfUser)
            => new SocketSelfUserAbstraction(socketSelfUser);
    }
}
