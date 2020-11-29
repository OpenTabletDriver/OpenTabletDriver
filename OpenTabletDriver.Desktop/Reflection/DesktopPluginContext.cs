using System;
using System.IO;
using System.Linq;
using OpenTabletDriver.Native;
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
        
        protected DirectoryInfo PluginDirectory { get; }

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
