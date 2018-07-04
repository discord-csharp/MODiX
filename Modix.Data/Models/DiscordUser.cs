using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Modix.Data.Models
{
    public class DiscordUser
    {
        [Required] public long DiscordUserID { get; set; }

        /// <summary>
        ///     Contains when the user joined Discord
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public string AvatarUrl { get; set; }
        public string Nickname { get; set; }
        public string Username { get; set; }
        public bool IsBot { get; set; }

        [InverseProperty("UserInQuestion")] public DbSet<Infraction> Infractions { get; set; }
    }
}