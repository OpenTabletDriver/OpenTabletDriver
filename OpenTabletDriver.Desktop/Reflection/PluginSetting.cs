using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSetting
    {
        public PluginSetting(string property, object value)
            : this()
        {
            PropertyName = property;
            Value = value;
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

        public string PropertyName { get; }

        public object Value { set; get; }

        public T GetValue<T>()
        {
            return Value is T targetValue ? targetValue : default(T);
        }
    }
}