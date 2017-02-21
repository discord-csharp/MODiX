using MongoDB.Bson.Serialization.Attributes;

namespace Modix.Data.Models
{
    public class GuildConfig : BaseModel
    {
        public GuildConfig()
        {
            CollectionName = nameof(GuildConfig);
        }

        [BsonElement("GuildId")]
        public ulong GuildId { get; set; }
        [BsonElement("AdminRoleId")]
        public ulong AdminRoleId { get; set; }
        [BsonElement("ModeratorRoleId")]
        public ulong ModeratorRoleId { get; set; }
    }
}
