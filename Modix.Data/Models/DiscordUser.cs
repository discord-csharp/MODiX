using System;

namespace Modix.Data.Models
{
    public class DiscordUser
    {
        public int Id { get; set; }
        public long DiscordId { get; set; }

        /// <summary>
        /// Contains when the user joined Discord
        /// </summary>
        public DateTime CreatedAt{ get; set; }
        public string AvatarUrl { get; set; }
        public string Nickname { get; set; }
        public string Username { get; set; }
        public bool IsBot { get; set; }
    }
}
