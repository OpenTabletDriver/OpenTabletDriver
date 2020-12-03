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
            if (value is string stringVal)
                Value = stringVal;
            else if (value is int intVal)
                Value = intVal;
            else if (value is uint uintVal)
                Value = uintVal;
            else if (value is float floatVal)
                Value = floatVal;
            else if (value is double doubleVal)
                Value = doubleVal;
            else if (value is Enum enumVal)
                Value = JObject.FromObject(enumVal);
            else
                Value = JObject.FromObject(value);
        }

        public T GetValue<T>()
        {
            return Value.ToObject<T>() ?? default(T);
        }

        public object GetValue(Type asType)
        {
            return Value.ToObject(asType);
        }
    }
}