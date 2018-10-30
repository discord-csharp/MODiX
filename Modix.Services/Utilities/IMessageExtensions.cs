using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Services.Utilities
{
    public static class IMessageExtensions
    {
        /// <summary>
        /// Return the guild, if any, this message was sent in
        /// </summary>
        /// <returns>An <see cref="IGuild"/> instance, or <see cref="null"/> if the message was not sent in an <see cref="IGuildChannel"/></returns>
        public static IGuild GetGuild(this IMessage message)
        {
            return (message.Channel as IGuildChannel)?.Guild;
        }

        /// <summary>
        /// Return the guild ID, if any, of the guild this message was sent in
        /// </summary>
        /// <returns>The <see cref="ulong"/> ID of the guild</returns>
        /// <exception cref="ArgumentException">Thrown if the message was not sent in an <see cref="IGuildChannel"/></exception>
        public static ulong GetGuildId(this IMessage message)
        {
            return GetGuild(message)?.Id ?? throw new ArgumentException("This message wasn't sent in an IGuildChannel");
        }

        /// <summary>
        /// Returns a permalink to the message, if it was sent in an <see cref="IGuildChannel"/>
        /// </summary>
        /// <param name="message">The message to get a permalink for</param>
        /// <returns>A URL pointing to the message</returns>
        /// <exception cref="ArgumentException">Thrown if the message was not sent in an <see cref="IGuildChannel"/></exception>
        public static string GetMessageLink(this IMessage message)
        {
            return ModixEmbedBuilderExtensions.MessageLink(message.GetGuildId(), message.Channel.Id, message.Id);
        }
    }
}
