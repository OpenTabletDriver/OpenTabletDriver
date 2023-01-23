using System.Reflection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Reflection;

namespace OpenTabletDriver.Daemon
{
    public static class Extensions
    {
        public static string? GetName(this IPluginFactory pluginFactory, PluginSettings? settingStore) => GetName(pluginFactory, settingStore?.Path);

        public static string? GetName(this IPluginFactory pluginFactory, string? path)
        {
            if (path == null)
                return null;

            var type = pluginFactory.GetPlugin(path);
            var attr = type?.GetCustomAttribute<PluginNameAttribute>();
            return attr != null ? attr.Name : type?.Name;
        }
    }
}
