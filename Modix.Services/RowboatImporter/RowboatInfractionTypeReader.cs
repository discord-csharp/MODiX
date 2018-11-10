using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modix.Services.RowboatImporter
{
    public class RowboatInfractionTypeReader : JsonConverter<RowboatInfractionType>
    {
        public override RowboatInfractionType ReadJson(JsonReader reader, Type objectType, RowboatInfractionType existingValue, bool hasExistingValue, JsonSerializer serializer)
            => JObject.Load(reader).GetValue("name").ToObject<RowboatInfractionType>();

        public override void WriteJson(JsonWriter writer, RowboatInfractionType value, JsonSerializer serializer) {}
    }
}
