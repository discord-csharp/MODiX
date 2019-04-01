namespace Discord.Rest
{
    /// <inheritdoc cref="RestGuildEmbed" />
    public interface IRestGuildEmbed
    {
        /// <inheritdoc cref="RestGuildEmbed.IsEnabled" />
        bool IsEnabled { get; }

        /// <inheritdoc cref="RestGuildEmbed.ChannelId" />
        ulong? ChannelId { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="RestGuildEmbed"/>, through the <see cref="IRestGuildEmbed"/> interface.
    /// </summary>
    public struct RestGuildEmbedAbstraction : IRestGuildEmbed
    {
        /// <summary>
        /// Constructs a new <see cref="RestGuildEmbedAbstraction"/> around an existing <see cref="RestGuildEmbed"/>.
        /// </summary>
        /// <param name="restGuildEmbed">The existing <see cref="RestGuildEmbed"/> to be abstracted.</param>
        public RestGuildEmbedAbstraction(RestGuildEmbed restGuildEmbed)
        {
            _restGuildEmbed = restGuildEmbed;
        }

        /// <inheritdoc />
        public ulong? ChannelId
            => _restGuildEmbed.ChannelId;

        /// <inheritdoc />
        public bool IsEnabled
            => _restGuildEmbed.IsEnabled;

        /// <inheritdoc cref="RestGuildEmbed.ToString" />
        public override string ToString()
            => _restGuildEmbed.ToString();

        private readonly RestGuildEmbed _restGuildEmbed;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestGuildEmbed"/> data.
    /// </summary>
    public static class RestGuildEmbedAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestGuildEmbed"/> to an abstracted <see cref="IRestGuildEmbed"/> value.
        /// </summary>
        /// <param name="restGuildEmbed">The existing <see cref="RestGuildEmbed"/> to be abstracted.</param>
        /// <returns>An <see cref="IRestGuildEmbed"/> that abstracts <paramref name="restGuildEmbed"/>.</returns>
        public static IRestGuildEmbed Abstract(this RestGuildEmbed restGuildEmbed)
            => new RestGuildEmbedAbstraction(restGuildEmbed);
    }
}
