using System.Linq;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;

namespace TabletDriverLib.Binding
{
    public static class BindingTools
    {
        public static IBinding GetBinding(string full)
        {
            if (!string.IsNullOrWhiteSpace(full))
            {
                var tokens = full.Contains(", ") ? full.Split(", ", 2) : full.Split(": ", 2);
                var binding = PluginManager.ConstructObject<IBinding>(tokens[0]);
                if (binding != null)
                    binding.Property = tokens[1];
                return binding;
            }
            else
            {
                return null;
            }
        }

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

        public static string GetBindingString(string name, string property)
        {
            return $"{name}: {property}";
        }
    }
}