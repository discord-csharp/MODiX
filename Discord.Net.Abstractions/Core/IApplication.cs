using System;

using Discord.Rest;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IApplication"/> objects.
    /// </summary>
    public static class ApplicationAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IApplication"/> to an abstracted <see cref="IApplication"/> value.
        /// </summary>
        /// <param name="application">The existing <see cref="IApplication"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="application"/>.</exception>
        /// <returns>An <see cref="IApplication"/> that abstracts <paramref name="application"/>.</returns>
        public static IApplication Abstract(this IApplication application)
            => application switch
            {
                null
                    => throw new ArgumentNullException(nameof(application)),
                RestApplication restApplication
                    => RestApplicationAbstractionExtensions.Abstract(restApplication),
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IApplication)} type {application.GetType().Name}")
            };
    }
}
