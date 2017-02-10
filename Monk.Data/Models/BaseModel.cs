using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monk.Data.Models
{
    public class BaseModel
    {
        public ObjectId Id { get; set; }

        [BsonIgnore]
        public string CollectionName { get; protected set; }
    }
}
