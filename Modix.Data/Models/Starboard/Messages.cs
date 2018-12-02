using Discord;

namespace Modix.Data.Models.Starboard
{
    public class Messages
    {
        /// <summary>
        /// Store Message ID
        /// </summary>
        public IMessage MessageId { get; set; }

        /// <summary>
        /// Store the number of stars for that message
        /// </summary>
        public int StarCount { get; set; }

        /// <summary>
        /// Store the user of the starred message
        /// </summary>
        public IGuildUser User { get; set; }
    }
}
