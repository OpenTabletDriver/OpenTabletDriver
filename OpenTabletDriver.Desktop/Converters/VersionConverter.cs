using System;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Converters
{
    public class VersionConverter : JsonConverter<Version>
    {
        public override Version? ReadJson(JsonReader reader, Type objectType, Version? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.String ? Version.Parse((string)reader.Value!) : null; // null-warning suppressed as string JSON token means the value exists in some capacity
        }

        public override void WriteJson(JsonWriter writer, Version? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
}
