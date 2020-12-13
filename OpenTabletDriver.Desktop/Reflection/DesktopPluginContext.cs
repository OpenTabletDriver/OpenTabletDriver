using System;
using System.IO;
using System.Linq;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class DesktopPluginContext : PluginContext
    {
        public DesktopPluginContext(DirectoryInfo directory)
        {
            PluginDirectory = directory;
        }

        public DesktopPluginContext(string directoryPath)
            : this(new DirectoryInfo(directoryPath))
        {
        }

        public DirectoryInfo PluginDirectory { get; }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            if (PluginDirectory == null)
            {
                Log.Write("Plugin", $"Independent plugin does not support loading native library '{unmanagedDllName}'", LogLevel.Warning);
                throw new NotSupportedException();
            }

            var runtimeFolder = new DirectoryInfo(Path.Join(PluginDirectory.FullName, "runtimes"));
            
            string libraryFile = null;
            if (runtimeFolder.Exists)
            {
                libraryFile = Directory.EnumerateFiles(
                    runtimeFolder.FullName,
                    ToDllName(unmanagedDllName),
                    SearchOption.AllDirectories
                ).FirstOrDefault();
                
            }
            if (!string.IsNullOrEmpty(libraryFile))
                return LoadUnmanagedDllFromPath(libraryFile);
            else
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
