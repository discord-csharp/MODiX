using Newtonsoft.Json;

namespace Modix.Services.RowboatImporter
{
    public class RowboatActor
    {
        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("bot")]
        public bool Bot { get; set; }

        [JsonProperty("discriminator")]
        public long Discriminator { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
