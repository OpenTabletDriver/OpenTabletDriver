using System;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSetting : NotifyPropertyChanged
    {
        public PluginSetting(string property, object value)
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

        private string _property = null!;
        private JToken? _value;

        [JsonProperty]
        public string Property
        {
            set => RaiseAndSetIfChanged(ref _property!, value);
            get => _property;
        }

        [JsonProperty]
        public JToken? Value
        {
            set => RaiseAndSetIfChanged(ref _value, value);
            get => _value;
        }

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
