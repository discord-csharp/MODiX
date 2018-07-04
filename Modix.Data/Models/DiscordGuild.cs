using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modix.Data.Models
{
    public class DiscordGuild
    {
        [Key, Required] public long GuildID { get; set; }

        [NotMapped]
        public ulong DiscordGuildID
        {
            get => (ulong) GuildID;
            set => GuildID = (long) value;
        }

        public string Name { get; set; }

        [Required] public DiscordUser Owner { get; set; }

        [Required] public DateTime CreatedAt { get; set; }

        public GuildConfig Config { get; set; }
    }
}