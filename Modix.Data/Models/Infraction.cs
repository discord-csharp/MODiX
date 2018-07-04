﻿using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Modix.Data.Models
{
    public class Infraction
    {
        [Key, Required] public long InfractionID { get; set; }

        [Required] public DiscordUser Creator { get; set; }

        [Required] public DiscordUser UserInQuestion { get; set; }

        public long Severity { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DiscordGuild GuildCreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime ToConcludeIncident { get; set; }
        public DateTime IncidentConcludedAt { get; set; }
        public DbSet<Ban> RelatedBans { get; set; }
    }
}