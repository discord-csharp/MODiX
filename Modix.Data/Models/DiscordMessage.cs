using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models
{
    public class DiscordMessage
    {
        [Key, Required]
        public long MessageId { get; set; }
        
        [NotMapped]
        public ulong DiscordMessageId { get => (ulong)MessageId; set => MessageId = (long)value; }

        public long DiscordId { get; set; }
        public DiscordGuild DiscordGuild { get; set; }
        public string Content { get; set; }
        public DiscordUser Author { get; set; }
        public string Attachments { get; set; }
    }
}