using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models
{
    public class DiscordGuildEntity
    {
        [Key, Required] public long GuildId { get; set; }

        [NotMapped]
        public ulong DiscordGuildId
        {
            get => (ulong) GuildId;
            set => GuildId = (long) value;
        }

        public string Name { get; set; }

        [Required] public DiscordUserEntity Owner { get; set; }

        [Required] public DateTime CreatedAt { get; set; }

        public GuildConfig Config { get; set; }
    }
}