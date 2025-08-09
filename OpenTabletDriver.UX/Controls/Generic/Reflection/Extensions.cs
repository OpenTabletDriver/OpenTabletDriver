using System.Reflection;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.UX.Controls.Generic.Reflection
{
    public static class Extensions
    {
        public static string GetFriendlyName(this TypeInfo type)
        {
            return type.GetCustomAttribute<PluginNameAttribute>()?.Name ?? type.FullName;
        }
    }
}
