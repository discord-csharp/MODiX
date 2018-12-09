using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="DiscordSocketClient" />
    public interface IDiscordSocketClient : IBaseSocketClient, IDiscordClient, IDisposable
    {
        /// <inheritdoc cref="DiscordSocketClient.GroupChannels" />
        IReadOnlyCollection<ISocketGroupChannel> GroupChannels { get; }

        /// <inheritdoc cref="DiscordSocketClient.DMChannels" />
        IReadOnlyCollection<ISocketDMChannel> DMChannels { get; }

        /// <inheritdoc cref="DiscordSocketClient.ShardId" />
        int ShardId { get; }

        /// <inheritdoc cref="DiscordSocketClient.Disconnected" />
        event Func<Exception, Task> Disconnected;

        /// <inheritdoc cref="DiscordSocketClient.Connected" />
        event Func<Task> Connected;

        /// <inheritdoc cref="DiscordSocketClient.LatencyUpdated" />
        event Func<int, int, Task> LatencyUpdated;

        /// <inheritdoc cref="DiscordSocketClient.Ready" />
        event Func<Task> Ready;
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around an <see cref="WebSocket.DiscordSocketClient"/>, through the <see cref="IDiscordSocketClient"/> interface.
    /// </summary>
    public class DiscordSocketClientAbstraction : BaseSocketClientAbstraction, IDiscordSocketClient
    {
        /// <summary>
        /// Constructs a new <see cref="DiscordSocketClientAbstraction"/> around an existing <see cref="WebSocket.DiscordSocketClient"/>.
        /// </summary>
        /// <param name="discordSocketClient">The value to use for <see cref="WebSocket.DiscordSocketClient"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordSocketClient"/>.</exception>
        public DiscordSocketClientAbstraction(DiscordSocketClient discordSocketClient)
            : base(discordSocketClient) { }

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketDMChannel> DMChannels
            => DiscordSocketClient.DMChannels
                .Select(SocketDMChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public IReadOnlyCollection<ISocketGroupChannel> GroupChannels
            => DiscordSocketClient.GroupChannels
                .Select(SocketGroupChannelAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public int ShardId
            => DiscordSocketClient.ShardId;

        /// <inheritdoc />
        public event Func<Task> Connected
        {
            add { DiscordSocketClient.Connected += value; }
            remove { DiscordSocketClient.Connected -= value; }
        }

        /// <inheritdoc />
        public event Func<Exception, Task> Disconnected
        {
            add { DiscordSocketClient.Disconnected += value; }
            remove { DiscordSocketClient.Disconnected -= value; }
        }

        /// <inheritdoc />
        public event Func<int, int, Task> LatencyUpdated
        {
            add { DiscordSocketClient.LatencyUpdated += value; }
            remove { DiscordSocketClient.LatencyUpdated -= value; }
        }

        /// <inheritdoc />
        public event Func<Task> Ready
        {
            add { DiscordSocketClient.Ready += value; }
            remove { DiscordSocketClient.Ready -= value; }
        }

        /// <summary>
        /// The existing <see cref="WebSocket.DiscordSocketClient"/> being abstracted.
        /// </summary>
        protected DiscordSocketClient DiscordSocketClient
            => BaseSocketClient as DiscordSocketClient;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="DiscordSocketClient"/> objects.
    /// </summary>
    public static class DiscordSocketClientAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="DiscordSocketClient"/> to an abstracted <see cref="IDiscordSocketClient"/> value.
        /// </summary>
        /// <param name="discordSocketClient">The existing <see cref="DiscordSocketClient"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordSocketClient"/>.</exception>
        /// <returns>An <see cref="IDiscordSocketClient"/> that abstracts <paramref name="discordSocketClient"/>.</returns>
        public static IDiscordSocketClient Abstract(this DiscordSocketClient discordSocketClient)
            => new DiscordSocketClientAbstraction(discordSocketClient);
    }
}



