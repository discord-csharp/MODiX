using System;

using Discord.Rest;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IVoiceRegion"/> objects.
    /// </summary>
    internal static class VoiceRegionAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IVoiceRegion"/> to an abstracted <see cref="IVoiceRegion"/> value.
        /// </summary>
        /// <param name="voiceRegion">The existing <see cref="IVoiceRegion"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="voiceRegion"/>.</exception>
        /// <returns>An <see cref="IVoiceRegion"/> that abstracts <paramref name="voiceRegion"/>.</returns>
        public static IVoiceRegion Abstract(this IVoiceRegion voiceRegion)
            => voiceRegion switch
            {
                null
                    => throw new ArgumentNullException(nameof(voiceRegion)),
                RestVoiceRegion restVoiceRegion
                    => RestVoiceRegionAbstractionExtensions.Abstract(restVoiceRegion) as IVoiceRegion,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IVoiceRegion)} type {voiceRegion.GetType().Name}")
            };
    }
}
