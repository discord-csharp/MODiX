using System.Text.Json.Serialization;
using Modix.Services.CodePaste;
using Modix.Services.StackExchange;
using Modix.Services.Wikipedia;

namespace Modix.Services
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(PasteResponse))]
    [JsonSerializable(typeof(StackExchangeResponse))]
    [JsonSerializable(typeof(WikipediaResponse))]
    internal partial class JsonMetadataContext : JsonSerializerContext
    {
    }
}
