using System;
using Newtonsoft.Json;

namespace OpenTabletDriver.Daemon.Converters
{
    public class VersionConverter : JsonConverter<Version>
    {
        public override Version ReadJson(JsonReader reader, Type objectType, Version? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new Version((string)reader.Value!);
        }

        public override void WriteJson(JsonWriter writer, Version? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
}
