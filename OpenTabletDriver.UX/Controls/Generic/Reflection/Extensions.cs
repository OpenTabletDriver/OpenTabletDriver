using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.UX.Controls.Generic.Reflection
{
    public static class Extensions
    {
        private static readonly Dictionary<TypeInfo, string> FriendlyNameCache = new();

        public static string GetFriendlyName(this TypeInfo type)
        {
            if (FriendlyNameCache.TryGetValue(type, out string? value))
                return value;

            string? rv = type.GetCustomAttribute<PluginNameAttribute>()?.Name ?? type.FullName;

            Debug.Assert(rv is not null, "Tried caching a null string");

            return FriendlyNameCache[type] = rv;
        }

        public static bool RemoveFromFriendlyNameCache(TypeInfo typeInfo) => FriendlyNameCache.Remove(typeInfo);

        public static void RemovePluginsFromFriendlyNameCache(IEnumerable<Assembly> assemblies)
        {
            foreach (var typeInfo in assemblies.GetPluginTypes().Select(x => x.GetTypeInfo()))
            {
                bool removed = RemoveFromFriendlyNameCache(typeInfo);
                if (removed)
                    Log.Write(nameof(FriendlyNameCache), $"Removed cached entry '{typeInfo.FullName}'", LogLevel.Debug);
            }
        }
    }
}
