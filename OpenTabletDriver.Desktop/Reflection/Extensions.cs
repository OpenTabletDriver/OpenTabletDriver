using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.Desktop.Reflection
{
    public static class Extensions
    {
        public static void CopyTo(this DirectoryInfo source, DirectoryInfo destination)
        {
            if (!source.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + source.FullName);
            }

            // If the destination directory doesn't exist, create it.
            destination.Create();

            // Get the files in the directory and copy them to the new location.
            foreach (var file in source.GetFiles())
            {
                string tempPath = Path.Combine(destination.FullName, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (DirectoryInfo subdir in source.GetDirectories())
            {
                CopyTo(
                    new DirectoryInfo(subdir.FullName),
                    new DirectoryInfo(Path.Combine(destination.FullName, subdir.Name))
                );
            }
        }

        public static T ConstructObject<T>(this PluginManager pluginManager, string name) where T : class
        {
            var provider = pluginManager.BuildServiceProvider();
            return ConstructObject<T>(provider, name);
        }

        public static T ConstructObject<T>(this IServiceProvider provider, string name) where T : class
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    return provider.GetRequiredService<T>(name);
                }
                catch (Exception e)
                {
                    Log.Write("Plugin", $"Unable to construct object '{name}'", LogLevel.Error);
                    Log.Exception(e);
                }
            }
            return null;
        }

        public static T GetRequiredService<T>(this IServiceProvider provider, string fullPath) where T : class
        {
            var type = AppInfo.PluginManager.GetTypeFromPath(fullPath);
            return provider.GetRequiredService(type) as T;
        }

        public static string GetFriendlyName(this Type type)
        {
            var attrs = type.GetCustomAttributes(true);
            var nameattr = attrs.FirstOrDefault(t => t.GetType() == typeof(PluginNameAttribute));
            return nameattr is PluginNameAttribute attr ? attr.Name : null;
        }

        public static IServiceCollection Clone(this IServiceCollection collection)
        {
            var clonedCollection = new ServiceCollection();

            foreach (var service in collection)
            {
                clonedCollection.Add(service);
            }

            return clonedCollection;
        }
    }
}