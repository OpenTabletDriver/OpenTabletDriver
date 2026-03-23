using System;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenTabletDriver.Daemon.Contracts.Persistence
{
    public class PluginSetting
    {
        public PluginSetting(string property, object? value)
        {
            Property = property;
            SetValue(value);
        }

        public PluginSetting(PropertyInfo property, object? value)
        {
            Property = property.Name;
            SetValue(value ?? GetDefault(property));
        }

        [JsonConstructor]
        private PluginSetting()
        {
        }

        [JsonProperty]
        public string Property { get; set; } = string.Empty;

        [JsonProperty]
        public JToken? Value { get; set; }

        public PluginSetting SetValue(object? value)
        {
            if (value is PluginSetting)
                throw new InvalidOperationException();

            Value = value == null ? null : JToken.FromObject(value);
            return this;
        }

        public T? GetValue<T>()
        {
            if (Value == null)
                return default;

            try
            {
                return Value.ToObject<T>();
            }
            catch
            {
                return default;
            }
        }

        public object? GetValue(Type asType)
        {
            if (Value == null)
                return default;

            try
            {
                return Value.ToObject(asType);
            }
            catch
            {
                return default;
            }
        }

        private static object? GetDefault(PropertyInfo property)
        {
            if (property.GetCustomAttribute<DefaultValueAttribute>() is { Value: object value } &&
                value.GetType() == property.PropertyType)
            {
                return value;
            }

            return null;
        }

        public override string ToString()
        {
            return Property + ": " + GetValue<object>();
        }
    }
}
