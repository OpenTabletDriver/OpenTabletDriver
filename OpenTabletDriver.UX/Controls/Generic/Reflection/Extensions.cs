using System.Collections.Generic;
using System.Reflection;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.UX.Controls.Generic.Reflection
{
    public static class Extensions
    {
        private static readonly Dictionary<TypeInfo, string> FriendlyNameCache = new();

        public static string GetFriendlyName(this TypeInfo type)
        {
            if (FriendlyNameCache.TryGetValue(type, out string value))
                return value;

            return FriendlyNameCache[type] = type.GetCustomAttribute<PluginNameAttribute>()?.Name ?? type.FullName;
        }

        public static void ClearFriendlyNameCache()
        {
            FriendlyNameCache.Clear();
        }
    }
}
