using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monk.Data.Models
{
    public class Cookie : BaseModel
    {
        public Cookie()
        {
            CollectionName = nameof(Cookie);
        }

        [BsonElement("GuildId")]
        public ulong GuildId { get; set; }
        [BsonElement("OwnerId")]
        public ulong OwnerId { get; set; }
        [BsonElement("Count")]
        public int Count { get; set; }
    }
}
