using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenTabletDriver.Desktop.Reflection
{
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
            Value = JToken.FromObject(value);
        }

        public T GetValue<T>()
        {
            return Value == null ? default(T) : Value.Type != JTokenType.Null ? Value.ToObject<T>() : default(T);
        }

        public object GetValue(Type asType)
        {
            return Value?.ToObject(asType);
        }
    }
}