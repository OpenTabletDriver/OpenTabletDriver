using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using OpenTabletDriver.Native;
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
            if (this.PluginPath == null)
            {
                Log.Write("Plugin", $"Independent plugin does not support loading native library '{unmanagedDllName}'", LogLevel.Warning);
                throw new NotImplementedException();
            }

            var runtimeFolder = new DirectoryInfo(Path.Join(this.PluginPath, "runtimes"));
            var libraryFile = runtimeFolder.Exists ? Directory.EnumerateFiles(runtimeFolder.FullName, ToDllName(unmanagedDllName), SearchOption.AllDirectories).FirstOrDefault() : null;
            if (!string.IsNullOrEmpty(libraryFile))
                return LoadUnmanagedDllFromPath(libraryFile);
            else
                return IntPtr.Zero;
        }

        private static string ToDllName(string dllName)
        {
            return SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => $"{dllName}.dll",
                RuntimePlatform.Linux => $"lib{dllName}.so",
                RuntimePlatform.MacOS => $"lib{dllName}.dylib",
                _ => null
            };
        }
    }
}