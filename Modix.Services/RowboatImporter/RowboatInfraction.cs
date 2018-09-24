using System;
using System.Collections.Generic;
using System.Text;
using Modix.Data.Models.Moderation;
using Newtonsoft.Json;

namespace Modix.Services.RowboatImporter
{
    public class RowboatInfraction
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("actor")]
        public RowboatActor Actor { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("expires_at")]
        public DateTimeOffset? ExpiresAt { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(RowboatInfractionTypeReader))]
        public RowboatInfractionType Type { get; set; }

        public InfractionType ModixInfractionType
        {
            get
            {
                switch (Type)
                {
                    case RowboatInfractionType.Ban:
                    case RowboatInfractionType.Tempban:
                    case RowboatInfractionType.Kick:
                        return InfractionType.Ban;

                    case RowboatInfractionType.Mute:
                    case RowboatInfractionType.Tempmute:
                        return InfractionType.Mute;

                    case RowboatInfractionType.Warning:
                        return InfractionType.Warning;

                    default:
                        return InfractionType.Notice;
                }
            }
        }

        [JsonProperty("user")]
        public RowboatActor User { get; set; }
    }
}
