using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models.Core
{
    public class DiscordMessageEntity
    {
        [Key, Required]
        public long DiscordMessageId { get; set; }

        public long DiscordId { get; set; }

        public DiscordGuildEntity DiscordGuild { get; set; }

        public string Content { get; set; }

        public DiscordUserEntity Author { get; set; }

        public string Attachments { get; set; }
    }
}