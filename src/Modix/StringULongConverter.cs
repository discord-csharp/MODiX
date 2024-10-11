using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;

namespace Modix
{
    public class StringULongConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == typeof(ulong);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert != typeof(ulong))
            {
                throw new ArgumentOutOfRangeException(nameof(typeToConvert), "JsonConverterFactory_TypeNotSupported");
            }

            return ULongConverterFactory.Create(typeToConvert, options);
        }
    }

    public class ULongConverterFactory
    {
        public static JsonConverter Create(Type typeToConvert, JsonSerializerOptions options)
            => new ULongConverter();
    }

    public class ULongConverter : JsonConverter<ulong>
    {
        public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}
