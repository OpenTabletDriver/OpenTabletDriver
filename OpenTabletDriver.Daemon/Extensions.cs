using System.Reflection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Desktop.Reflection;

#nullable enable

namespace OpenTabletDriver.Daemon
{
    public static class Extensions
    {
        public static string? GetName(this IPluginFactory pluginFactory, PluginSettings? settingStore) => GetName(pluginFactory, settingStore?.Path);

        public static string? GetName(this IPluginFactory pluginFactory, string? path)
        {
            if (path == null)
                return null;

            var type = pluginFactory.GetPluginType(path);
            var attr = type?.GetCustomAttribute<PluginNameAttribute>();
            return attr != null ? attr.Name : type?.Name;
        }
    }
}
