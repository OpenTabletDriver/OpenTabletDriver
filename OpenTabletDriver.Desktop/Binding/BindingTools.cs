using System.Linq;
using System.Text.RegularExpressions;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop.Binding
{
    public static class BindingTools
    {
        public static IBinding GetBinding(string full)
        {
            if (!string.IsNullOrWhiteSpace(full))
            {
                var tokens = full.Contains(", ") ? full.Split(", ", 2) : full.Split(": ", 2);
                var pluginRef = new PluginReference(tokens[0]);
                var binding = pluginRef.Construct<IBinding>();
                if (binding != null)
                    binding.Property = tokens[1];
                return binding;
            }
            else
            {
                return null;
            }
        }

        public const string BindingRegexExpression = "^(?<BindingPath>.+?): (?<BindingProperty>.+?)$";

        public static string GetBindingString<T>(T binding) where T : IBinding
        {
            return GetBindingString(typeof(T).FullName, binding.Property);
        }

        public static string GetShortBindingString<T>(T binding) where T : IBinding
        {
            var type = typeof(T);
            var attrs = type.GetCustomAttributes(false);
            var name = attrs.FirstOrDefault(t => t is PluginNameAttribute) is PluginNameAttribute nameAttr ? nameAttr.Name : type.Name;
            return GetBindingString(name, binding.Property);
        }

        public static string GetBindingPath(string full)
        {
            var bindingRegex = new Regex(BindingRegexExpression);
            var match = bindingRegex.Match(full);
            return match.Success ? match.Groups["BindingPath"].Value : null;
        }

        public static string GetBindingProperty(string full)
        {
            var bindingRegex = new Regex(BindingRegexExpression);
            var match = bindingRegex.Match(full);
            return match.Success ? match.Groups["BindingProperty"].Value : null;
        }

        public static string GetBindingString(string name, string property)
        {
            return $"{name}: {property}";
        }
    }
}
