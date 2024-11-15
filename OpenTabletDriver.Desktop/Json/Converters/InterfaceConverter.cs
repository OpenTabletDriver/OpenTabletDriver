using System;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Json.Converters
{
    public class InterfaceConverter<TInterface, TClass> : JsonConverter<TInterface> where TInterface : class where TClass : TInterface, new()
    {
        public override void WriteJson(JsonWriter writer, TInterface? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override TInterface ReadJson(JsonReader reader, Type objectType, TInterface? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var obj = existingValue ?? new TClass();
            serializer.Populate(reader, obj);
            return obj;
        }
    }
}
