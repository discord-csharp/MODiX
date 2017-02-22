using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Modix.Data.Models
{
    public class Message : BaseModel
    {
        public Message()
        {
            CollectionName = nameof(Message);
        }
        [BsonElement("MessageId")]
        public ulong MessageId { get; set; }
        [BsonElement("GuildId")]
        public ulong GuildId { get; set; }
        [BsonElement("Content")]
        public string Content { get; set; }
        [BsonElement("IsBot")]
        public bool IsBot { get; set; }
        [BsonElement("Username")]
        public string Username { get; set; }
        [BsonElement("DiscriminatorValue")]
        public ushort? DiscriminatorValue { get; set; }
        [BsonElement("AvatarId")]
        public string AvatarId { get; set; }
        [BsonElement("AvatarUrl")]
        public string AvatarUrl { get; set; }
        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }
        [BsonElement("Discriminator")]
        public string Discriminator { get; set; }
        [BsonElement("Mention")]
        public string Mention { get; set; }
        [BsonElement("Game")]
        public string Game { get; set; }
        [BsonElement("Attachments")]
        public string[] Attachments { get; set; }
    }
}
