using System.Text.Json.Serialization;
using Modix.Bot.Modules;

namespace Modix.Bot
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(ReplResult))]
    internal partial class JsonMetadataContext : JsonSerializerContext
    {
    }
}
