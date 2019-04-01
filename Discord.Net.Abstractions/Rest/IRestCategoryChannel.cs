using System;

namespace Discord.Rest
{
    /// <inheritdoc cref="RestCategoryChannel" />
    public interface IRestCategoryChannel : IRestGuildChannel, ICategoryChannel { }

    /// <summary>
    /// Provides an abstraction wrapper layer around a <see cref="Rest.RestCategoryChannel"/>, through the <see cref="IRestCategoryChannel"/> interface.
    /// </summary>
    public class RestCategoryChannelAbstraction : RestGuildChannelAbstraction, IRestCategoryChannel
    {
        /// <summary>
        /// Constructs a new <see cref="RestCategoryChannelAbstraction"/> around an existing <see cref="Rest.RestCategoryChannel"/>.
        /// </summary>
        /// <param name="restCategoryChannel">The value to use for <see cref="Rest.RestCategoryChannel"/>.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restCategoryChannel"/>.</exception>
        public RestCategoryChannelAbstraction(RestCategoryChannel restCategoryChannel)
            : base(restCategoryChannel) { }
    }

    /// <summary>
    /// Contains extension methods for abstracting <see cref="RestCategoryChannel"/> objects.
    /// </summary>
    public static class RestCategoryChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="RestCategoryChannel"/> to an abstracted <see cref="IRestCategoryChannel"/> value.
        /// </summary>
        /// <param name="restCategoryChannel">The existing <see cref="RestCategoryChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="restCategoryChannel"/>.</exception>
        /// <returns>An <see cref="IRestCategoryChannel"/> that abstracts <paramref name="restCategoryChannel"/>.</returns>
        public static IRestCategoryChannel Abstract(this RestCategoryChannel restCategoryChannel)
            => new RestCategoryChannelAbstraction(restCategoryChannel);
    }
}
