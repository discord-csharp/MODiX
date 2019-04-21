using System;
using System.Linq.Expressions;
using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    public class MessageBrief
    {
        /// <summary>
        /// The unique Discord snowflake ID of the guild in which the message was sent.
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the channel in which the message was sent.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// The Discord ID of the message.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID of the user who sent the message.
        /// </summary>
        public ulong AuthorId { get; set; }

        /// <summary>
        /// A timestamp indicating when the message was sent.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The unique Discord snowflake ID for the starboard-message associated with this message.
        /// </summary>
        public ulong? StarboardEntryId { get; internal set; }

        internal static readonly Expression<Func<MessageEntity, MessageBrief>> FromEntityProjection
            = entity => new MessageBrief()
            {
                Id = entity.Id,
                GuildId = entity.GuildId,
                ChannelId = entity.ChannelId,
                AuthorId = entity.AuthorId,
                Timestamp = entity.Timestamp,
                StarboardEntryId = entity.StarboardEntryId,
            };
    }
}
