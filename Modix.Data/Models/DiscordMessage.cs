using System;
using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models
{
    public class DiscordMessage
    {
        public int Id { get; set; }
        public long DiscordId { get; set; }
        public DiscordGuild DiscordGuild { get; set; }
        public string Content { get; set; }
        public DiscordUser Author { get; set; }
        public string Attachments { get; set; }
    }
}
