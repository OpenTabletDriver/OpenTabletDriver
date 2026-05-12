using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class DesktopPluginContext : PluginContext
    {
        public DesktopPluginContext(DirectoryInfo directory)
        {
            Directory = directory;
            FriendlyName = Directory.Name;

            foreach (var plugin in Directory.EnumerateFiles("*.dll"))
            {
                // Ignore a plugin library build artifact
                // Loading it seems to stop loading any further DLLs from the directory
                if (string.Equals(plugin.Name, "OpenTabletDriver.Plugin.dll", StringComparison.OrdinalIgnoreCase))
                    continue;

                LoadAssemblyFromFile(plugin);
            }
        }

        public DirectoryInfo Directory { get; }

        public string FriendlyName { get; }

        public PluginMetadata GetMetadata()
        {
            Directory.Refresh();
            if (Directory.Exists && Directory.EnumerateFiles().FirstOrDefault(f => f.Name == "metadata.json") is FileInfo file)
            {
                var metadata = Serialization.Deserialize<PluginMetadata>(file)
                    ?? throw new InvalidOperationException($"Could not deserialize {nameof(PluginMetadata)} for {file.Name}");

                metadata.Installed = true;
                return metadata;
            }
            else
            {
                return new PluginMetadata
                {
                    Name = FriendlyName,
                    Owner = "<unknown>",
                    SupportedDriverVersion = Assembly.GetEntryAssembly()!.GetName().Version!, // TODO: require plugin manifest to ensure compatibility?
                    Installed = true,
                };
            }
        }

        protected Assembly? LoadAssemblyFromFile(FileInfo file)
        {
            try
            {
                var stream = file.OpenRead();
                var asm = LoadFromStream(stream);
                stream.Close();
                return asm;
            }
            catch
            {
                Log.Write("Plugin", $"Failed loading assembly '{file.Name}'", LogLevel.Error);
                return null;
            }
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
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
                _ => throw new InvalidOperationException($"Unsupported plugin platform '{SystemInterop.CurrentPlatform}'"),
            };
        }
    }
}
