using System;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestVoiceRegion" />
    public interface IRestVoiceRegion : IEntity<string>, IVoiceRegion { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="RestVoiceRegion"/>, through the <see cref="IRestVoiceRegion"/> interface.
    /// </summary>
    internal class RestVoiceRegionAbstraction : IRestVoiceRegion
    {
        /// <summary>
        /// Constructs a new <see cref="RestVoiceRegionAbstraction"/> around an existing <see cref="Rest.RestVoiceRegion"/>.
        /// </summary>
        /// <param name="restVoiceRegion">The value to use for <see cref="Rest.RestVoiceRegion"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restVoiceRegion"/>.</exception>
        public RestVoiceRegionAbstraction(RestVoiceRegion restVoiceRegion)
        {
            RestVoiceRegion = restVoiceRegion ?? throw new ArgumentNullException(nameof(restVoiceRegion));
        }

        /// <inheritdoc />
        public string Name
            => RestVoiceRegion.Name;

        /// <inheritdoc />
        public string Id
            => RestVoiceRegion.Id;

        /// <inheritdoc />
        public bool IsVip
            => RestVoiceRegion.IsVip;

        /// <inheritdoc />
        public bool IsOptimal
            => RestVoiceRegion.IsOptimal;

        /// <inheritdoc />
        public bool IsDeprecated
            => RestVoiceRegion.IsDeprecated;

        /// <inheritdoc />
        public bool IsCustom
            => RestVoiceRegion.IsCustom;

        /// <inheritdoc cref="RestVoiceRegion.ToString" />
        public override string ToString()
            => RestVoiceRegion.ToString();

        /// <summary>
        /// The existing <see cref="Rest.RestVoiceRegion"/> being abstracted.
        /// </summary>
        protected RestVoiceRegion RestVoiceRegion { get; }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestVoiceRegion"/> objects.
    /// </summary>
    internal static class RestVoiceRegionAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestVoiceRegion"/> to an abstracted <see cref="IRestVoiceRegion"/> value.
        /// </summary>
        /// <param name="restVoiceRegion">The existing <see cref="RestVoiceRegion"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restVoiceRegion"/>.</exception>
        /// <returns>An <see cref="IRestVoiceRegion"/> that abstracts <paramref name="restVoiceRegion"/>.</returns>
        public static IRestVoiceRegion Abstract(this RestVoiceRegion restVoiceRegion)
            => new RestVoiceRegionAbstraction(restVoiceRegion);
    }
}
