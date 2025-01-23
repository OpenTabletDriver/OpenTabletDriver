using System.Reflection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.Daemon.Library.Reflection;

namespace OpenTabletDriver.Daemon.Library
{
    public static class Extensions
    {
        public static string? GetName(this IPluginFactory pluginFactory, PluginSettings? settingStore) => GetName(pluginFactory, settingStore?.Path);

        public static string? GetName(this IPluginFactory pluginFactory, string? path)
        {
            if (path == null)
                return null;

            var plugin = pluginFactory.GetPlugin(path);
            var attr = plugin?.GetCustomAttribute<PluginNameAttribute>();
            return attr != null ? attr.Name : plugin?.Name;
        }
    }
}
