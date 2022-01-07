using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

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
        public string Property { set; get; }

        [JsonProperty]
        public JToken Value { set; get; }

        [JsonIgnore]
        public bool HasValue => Value != null && Value.Type != JTokenType.Null;

        public void SetValue(object value)
        {
            Value = value == null ? null : JToken.FromObject(value);
        }

        public T GetValue<T>()
        {
            return Value == null ? default(T) : Value.Type != JTokenType.Null ? Value.ToObject<T>() : default(T);
        }

        public object GetValue(Type asType)
        {
            return Value == null ? default : Value.Type != JTokenType.Null ? Value.ToObject(asType) : default;
        }

        public T GetValueOrDefault<T>(PropertyInfo property)
        {
            if (this.HasValue)
            {
                return GetValue<T>();
            }
            else
            {
                if (property.GetCustomAttribute<DefaultPropertyValueAttribute>() is DefaultPropertyValueAttribute defaults)
                {
                    try
                    {
                        SetValue(defaults.Value);
                        return (T)defaults.Value;
                    }
                    catch (Exception e)
                    {
                        Log.Write(nameof(PluginSetting), $"Failed to get custom default of {property.Name}: {e.Message}");
                    }
                }
                return default;
            }
        }
    }
}
