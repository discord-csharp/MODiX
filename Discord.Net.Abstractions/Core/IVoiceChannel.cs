using System;

using Discord.Rest;
using Discord.WebSocket;

namespace Discord
{
    /// <summary>
    /// Contains extension methods for abstracting <see cref="IVoiceChannel"/> objects.
    /// </summary>
    internal static class VoiceChannelAbstractionExtensions
    {
        /// <summary>
        /// Converts an existing <see cref="IVoiceChannel"/> to an abstracted <see cref="IVoiceChannel"/> value.
        /// </summary>
        /// <param name="voiceChannel">The existing <see cref="IVoiceChannel"/> to be abstracted.</param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="voiceChannel"/>.</exception>
        /// <returns>An <see cref="IVoiceChannel"/> that abstracts <paramref name="voiceChannel"/>.</returns>
        public static IVoiceChannel Abstract(this IVoiceChannel voiceChannel)
            => voiceChannel switch
            {
                null
                    => throw new ArgumentNullException(nameof(voiceChannel)),
                RestVoiceChannel restVoiceChannel
                    => RestVoiceChannelAbstractionExtensions.Abstract(restVoiceChannel) as IVoiceChannel,
                SocketVoiceChannel socketVoiceChannel
                    => SocketVoiceChannelAbstractionExtensions.Abstract(socketVoiceChannel) as IVoiceChannel,
                _
                    => throw new NotSupportedException($"Unable to abstract {nameof(IVoiceChannel)} type {voiceChannel.GetType().Name}")
            };
    }
}
