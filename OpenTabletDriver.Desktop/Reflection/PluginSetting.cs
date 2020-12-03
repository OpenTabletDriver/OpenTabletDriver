using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenTabletDriver.Desktop.Reflection
{
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class PluginSetting
    {
        public PluginSetting(string property, object value)
            : this()
        {
            Property = property;
            SetValue(value);
        }

        public PluginSetting(PropertyInfo property, object value)
            : this(property.Name, value)
        {
        }

        public PluginSetting(PropertyInfo property)
            : this(property, null)
        {
        }

        [JsonConstructor]
        private PluginSetting()
        {
        }

        [JsonProperty]
        public string Property { protected set; get; }

        [JsonProperty]
        public JToken Value { set; get; }

        public void SetValue(object value)
        {
            Value = value switch
            {
                string val => val,
                bool val => val,
                int val => val,
                uint val => val,
                float val => val,
                double val => val,
                DateTime val => val,
                TimeSpan val => val,
                Guid val => val,
                Uri val => val,
                Enum val => JObject.FromObject(val),
                _ => value != null ? JObject.FromObject(value) : null
            };
        }

        public T GetValue<T>()
        {
            return Value != null ? Value.ToObject<T>() : default(T);
        }

        public object GetValue(Type asType)
        {
            return Value?.ToObject(asType);
        }
    }
}