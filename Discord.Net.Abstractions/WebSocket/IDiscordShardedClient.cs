using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    /// <inheritdoc cref="DiscordShardedClient" />
    public interface IDiscordShardedClient : IBaseSocketClient
    {
        /// <inheritdoc cref="DiscordShardedClient.Shards" />
        IReadOnlyCollection<IDiscordSocketClient> Shards { get; }

        /// <inheritdoc cref="DiscordShardedClient.ShardDisconnected" />
        event Func<Exception, IDiscordSocketClient, Task> ShardDisconnected;

        /// <inheritdoc cref="DiscordShardedClient.ShardConnected" />
        event Func<IDiscordSocketClient, Task> ShardConnected;

        /// <inheritdoc cref="DiscordShardedClient.ShardReady" />
        event Func<IDiscordSocketClient, Task> ShardReady;

        /// <inheritdoc cref="DiscordShardedClient.ShardLatencyUpdated" />
        event Func<int, int, IDiscordSocketClient, Task> ShardLatencyUpdated;

        /// <inheritdoc cref="DiscordShardedClient.GetShard(int)" />
        IDiscordSocketClient GetShard(int id);

        /// <inheritdoc cref="DiscordShardedClient.GetShardFor(IGuild)" />
        IDiscordSocketClient GetShardFor(IGuild guild);
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around an <see cref="WebSocket.DiscordShardedClient"/>, through the <see cref="IDiscordShardedClient"/> interface.
    /// </summary>
    internal class DiscordShardedClientAbstraction : BaseSocketClientAbstraction, IDiscordShardedClient
    {
        /// <summary>
        /// Constructs a new <see cref="DiscordShardedClientAbstraction"/> around an existing <see cref="WebSocket.DiscordShardedClient"/>.
        /// </summary>
        /// <param name="discordShardedClient">The value to use for <see cref="WebSocket.DiscordShardedClient"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordShardedClient"/>.</exception>
        public DiscordShardedClientAbstraction(DiscordShardedClient discordShardedClient)
            : base(discordShardedClient)
        {
            discordShardedClient.ShardConnected += x => ShardConnected?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
            discordShardedClient.ShardDisconnected += (x, y) => ShardDisconnected?.InvokeAsync(x, y.Abstract()) ?? Task.CompletedTask;
            discordShardedClient.ShardLatencyUpdated += (x, y, z) => ShardLatencyUpdated?.InvokeAsync(x, y, z.Abstract()) ?? Task.CompletedTask;
            discordShardedClient.ShardReady += x => ShardReady?.InvokeAsync(x.Abstract()) ?? Task.CompletedTask;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IDiscordSocketClient> Shards
            => DiscordShardedClient.Shards
                .Select(DiscordSocketClientAbstractionExtensions.Abstract)
                .ToArray();

        /// <inheritdoc />
        public event Func<IDiscordSocketClient, Task> ShardConnected;

        /// <inheritdoc />
        public event Func<Exception, IDiscordSocketClient, Task> ShardDisconnected;

        /// <inheritdoc />
        public event Func<int, int, IDiscordSocketClient, Task> ShardLatencyUpdated;

        /// <inheritdoc />
        public event Func<IDiscordSocketClient, Task> ShardReady;

        /// <inheritdoc />
        public IDiscordSocketClient GetShard(int id)
            => DiscordShardedClient.GetShard(id)
                .Abstract();

        /// <inheritdoc />
        public IDiscordSocketClient GetShardFor(IGuild guild)
            => DiscordShardedClient.GetShardFor(guild)
                .Abstract();

        /// <summary>
        /// The existing <see cref="WebSocket.DiscordShardedClient"/> being abstracted.
        /// </summary>
        protected DiscordShardedClient DiscordShardedClient
            => BaseSocketClient as DiscordShardedClient;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="DiscordShardedClient"/> objects.
    /// </summary>
    public static class DiscordShardedClientAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="DiscordShardedClient"/> to an abstracted <see cref="IDiscordShardedClient"/> value.
        /// </summary>
        /// <param name="discordShardedClient">The existing <see cref="DiscordShardedClient"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="discordShardedClient"/>.</exception>
        /// <returns>An <see cref="IDiscordShardedClient"/> that abstracts <paramref name="discordShardedClient"/>.</returns>
        public static IDiscordShardedClient Abstract(this DiscordShardedClient discordShardedClient)
            => new DiscordShardedClientAbstraction(discordShardedClient);
    }
}
