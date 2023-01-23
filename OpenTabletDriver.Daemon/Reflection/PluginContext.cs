using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Interop;

namespace OpenTabletDriver.Daemon.Reflection
{
    public class PluginContext : AssemblyLoadContext
    {
        private readonly IPluginManager _manager;
        private ImmutableArray<Type> _plugins = ImmutableArray<Type>.Empty;

        public PluginContext(IPluginManager manager, DirectoryInfo directory)
        {
            _manager = manager;
            Directory = directory;
            FriendlyName = Directory.Name;

            foreach (var plugin in Directory.EnumerateFiles("*.dll"))
            {
                // Ignore a plugin library build artifact
                // Loading it seems to stop loading any further DLLs from the directory
                if (string.Equals(plugin.Name, "OpenTabletDriver.dll", StringComparison.OrdinalIgnoreCase))
                    continue;

                LoadAssemblyFromFile(plugin);
            }

            Metadata = GetMetadata();
        }

        public DirectoryInfo Directory { get; }
        public string FriendlyName { get; }
        public ImmutableArray<Type> PluginTypes => _plugins;
        public PluginMetadata Metadata { get; }

        private PluginMetadata GetMetadata()
        {
            Directory.Refresh();
            if (Directory.Exists && Directory.EnumerateFiles().FirstOrDefault(f => f.Name == "metadata.json") is FileInfo file)
            {
                return Serialization.Deserialize<PluginMetadata>(file)!;
            }

            return new PluginMetadata
            {
                Name = FriendlyName
            };
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Reference shared dependency instead of duplicating it into this PluginContext
            return assemblyName.Name == PluginManager.PLUGIN_ASSEMBLY_NAMESPACE ? PluginManager.PluginAssembly : null;
        }

        private Assembly? LoadAssemblyFromFile(FileSystemInfo file)
        {
            try
            {
                var assembly = LoadFromAssemblyPath(file.FullName);
                var pluginTypes = from type in assembly.ExportedTypes
                    where _manager.IsLoadablePluginType(type)
                    select type;

                ImmutableInterlocked.Update(ref _plugins, p => p.AddRange(pluginTypes));

                return assembly;
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
                Log.Write("Plugin", $"Independent plugin does not support loading native library '{unmanagedDllName}'",
                    LogLevel.Warning);
                throw new NotSupportedException();
            }

            var runtimeFolder = new DirectoryInfo(Path.Join(Directory.FullName, "runtimes"));
            if (runtimeFolder.Exists)
            {
                var libraryFile = runtimeFolder.EnumerateFiles(ToDllName(unmanagedDllName), SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (libraryFile != null)
                    return LoadUnmanagedDllFromPath(libraryFile.FullName);
            }

            return IntPtr.Zero;
        }

        private static string ToDllName(string dllName)
        {
            return SystemInterop.CurrentPlatform switch
            {
                SystemPlatform.Windows => $"{dllName}.dll",
                SystemPlatform.Linux => $"lib{dllName}.so",
                SystemPlatform.MacOS => $"lib{dllName}.dylib",
                _ => throw new PlatformNotSupportedException()
            };
        }
    }
}
