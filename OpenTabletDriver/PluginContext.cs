using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver
{
    public class PluginContext : AssemblyLoadContext
    {
        private static Assembly OTDPlugin;

        public readonly string PluginName;
        public readonly string PluginPath;

        public PluginContext(string pluginName = null)
        {
            if (OTDPlugin == null)
            {
                foreach (var assembly in Default.Assemblies)
                {
                    if (assembly.GetName().ToString() == "OpenTabletDriver.Plugin")
                        OTDPlugin = assembly;
                }
            }

            this.PluginName = pluginName;

            if (pluginName != null)
                this.PluginPath = Path.Join(AppInfo.Current.PluginDirectory, pluginName);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name == "OpenTabletDriver.Plugin")
            {
                // Reference shared dependency instead of duplicating it into this PluginContext
                return OTDPlugin;
            }
            else
            {
                return null;
            }
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            // Try to load unmanaged dll using the default behaviour
            var baseLoad = base.LoadUnmanagedDll(unmanagedDllName);
            if (baseLoad != IntPtr.Zero)
                return baseLoad;

            // Default behaviour failed, search <plugin>/runtimes for the dll
            if (this.PluginPath == null)
            {
                PluginManager.Log($"Obsolete plugin does not support loading native library '{unmanagedDllName}'", LogLevel.Warning);
                return IntPtr.Zero;
            }

            var libraryFile = Directory.EnumerateFiles(Path.Join(this.PluginPath, "runtimes"), $"{unmanagedDllName}.*", SearchOption.AllDirectories).FirstOrDefault();
            return LoadUnmanagedDllFromPath(libraryFile);
        }
    }
}