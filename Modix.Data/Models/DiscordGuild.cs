using System;
using System.ComponentModel.DataAnnotations;

namespace Modix.Data.Models
{
    public class DiscordGuild
    {
        public int Id { get; set; }

        [Required]
        public long DiscordId { get; set; }
        public string Name { get; set; }
        [Required]
        public DiscordUser Owner { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public GuildConfig Config { get; set; }
    }
}
