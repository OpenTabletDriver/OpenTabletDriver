using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class DesktopPluginContext : PluginContext
    {
        public DesktopPluginContext(DirectoryInfo directory)
        {
            Directory = directory;
            FriendlyName = Directory.Name;

            foreach (var plugin in Directory.EnumerateFiles("*.dll"))
                LoadAssemblyFromFile(plugin);
        }

        public DirectoryInfo Directory { get; }

        public string FriendlyName { get; }

        public PluginMetadata GetMetadata()
        {
            if (Directory.EnumerateFiles().FirstOrDefault(f => f.Name == "metadata.json") is FileInfo file)
            {
                return Serialization.Deserialize<PluginMetadata>(file);
            }
            else
            {
                return null;
            }
        }

        protected Assembly LoadAssemblyFromFile(FileInfo file)
        {
            try
            {
                return LoadFromAssemblyPath(file.FullName);
            }
            catch
            {
                Log.Write("Plugin", $"Failed loading assembly '{file.Name}'", LogLevel.Error);
                return null;
            }
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            if (Directory == null)
            {
                Log.Write("Plugin", $"Independent plugin does not support loading native library '{unmanagedDllName}'", LogLevel.Warning);
                throw new NotSupportedException();
            }

            var runtimeFolder = new DirectoryInfo(Path.Join(Directory.FullName, "runtimes"));
            if (runtimeFolder.Exists)
            {
                var libraryFile = runtimeFolder.EnumerateFiles(ToDllName(unmanagedDllName), SearchOption.AllDirectories).FirstOrDefault();
                if (libraryFile != null)
                    return LoadUnmanagedDllFromPath(libraryFile.FullName);
            }
            return IntPtr.Zero;
        }

        private static string ToDllName(string dllName)
        {
            return SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => $"{dllName}.dll",
                PluginPlatform.Linux => $"lib{dllName}.so",
                PluginPlatform.MacOS => $"lib{dllName}.dylib",
                _ => null
            };
        }
    }
}
