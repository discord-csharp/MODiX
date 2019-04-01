namespace Discord
{
    /// <inheritdoc cref="ReactionMetadata" />
    public interface IReactionMetadata
    {
        /// <inheritdoc cref="ReactionMetadata.ReactionCount" />
        int ReactionCount { get; }

        /// <inheritdoc cref="ReactionMetadata.IsMe" />
        bool IsMe { get; }
    }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="ReactionMetadata"/>, through the <see cref="IReactionMetadata"/> interface.
    /// </summary>
    public struct ReactionMetadataAbstraction : IReactionMetadata
    {
        /// <summary>
        /// Constructs a new <see cref="ReactionMetadataAbstraction"/> around an existing <see cref="ReactionMetadata"/>.
        /// </summary>
        /// <param name="reactionMetadata">The existing <see cref="ReactionMetadata"/> to be abstracted.</param>
        public ReactionMetadataAbstraction(ReactionMetadata reactionMetadata)
        {
            _reactionMetadata = reactionMetadata;
        }

        /// <inheritdoc />
        public int ReactionCount
            => _reactionMetadata.ReactionCount;

        /// <inheritdoc />
        public bool IsMe
            => _reactionMetadata.IsMe;

        private readonly ReactionMetadata _reactionMetadata;
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="ReactionMetadata"/> data.
    /// </summary>
    public static class ReactionMetadataAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="ReactionMetadata"/> to an abstracted <see cref="IReactionMetadata"/> value.
        /// </summary>
        /// <param name="reactionMetadata">The existing <see cref="ReactionMetadata"/> to be abstracted.</param>
        /// <returns>An <see cref="IReactionMetadata"/> that abstracts <paramref name="reactionMetadata"/>.</returns>
        public static IReactionMetadata Abstract(this ReactionMetadata reactionMetadata)
            => new ReactionMetadataAbstraction(reactionMetadata);
    }
}
