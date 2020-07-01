using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Comlink.Json
{
    public class ObjectConverter : JsonConverter<Object?>
    {
        public override Object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.TryGetInt64(out long l) ? l : reader.GetDouble(),
            JsonTokenType.String => reader.TryGetDateTime(out DateTime datetime) ? (Object?)datetime : reader.GetString(),
            _ => JsonDocument.ParseValue(ref reader).RootElement.Clone(),
        };

        public override void Write(Utf8JsonWriter writer, Object? value, JsonSerializerOptions options)
        {
            throw new InvalidOperationException("Should not get here.");
        }
    }
}