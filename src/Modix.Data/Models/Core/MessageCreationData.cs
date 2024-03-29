using System;
using Modix.Data.Models.Core;

namespace Modix.Data.Repositories
{
    public class MessageCreationData
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

        internal MessageEntity ToEntity()
            => new MessageEntity()
            {
                GuildId = GuildId,
                ChannelId = ChannelId,
                Id = Id,
                AuthorId = AuthorId,
                Timestamp = Timestamp,
            };
    }
}
