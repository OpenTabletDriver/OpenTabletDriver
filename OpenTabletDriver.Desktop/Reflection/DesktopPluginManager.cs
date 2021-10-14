using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Input;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class DesktopPluginManager : PluginManager
    {
        public DesktopPluginManager()
            : this(AppInfo.Current.PluginDirectory, AppInfo.Current.TrashDirectory, AppInfo.Current.TemporaryDirectory)
        {
        }

        protected DesktopPluginManager(string pluginDirectory, string trashDirectory, string tempDirectory)
            : this(new DirectoryInfo(pluginDirectory), new DirectoryInfo(trashDirectory), new DirectoryInfo(tempDirectory))
        {
        }

        public DesktopPluginManager(DirectoryInfo pluginDirectory, DirectoryInfo trashDirectory, DirectoryInfo tempDirectory)
        {
            PluginDirectory = pluginDirectory;
            TrashDirectory = trashDirectory;
            TemporaryDirectory = tempDirectory;

            // These services will always be provided on the desktop
            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.Windows:
                {
                    this.AddTransient<ITimer, WindowsTimer>();
                    this.AddTransient<IAbsolutePointer, WindowsAbsolutePointer>();
                    this.AddTransient<IRelativePointer, WindowsRelativePointer>();
                    this.AddTransient<IVirtualKeyboard, WindowsVirtualKeyboard>();
                    this.AddTransient<IVirtualScreen, WindowsDisplay>();
                    break;
                }
                case PluginPlatform.Linux:
                {
                    this.AddTransient<ITimer, LinuxTimer>();
                    this.AddSingleton<IAbsolutePointer, EvdevAbsolutePointer>();
                    this.AddSingleton<IRelativePointer, EvdevRelativePointer>();
                    this.AddSingleton<IVirtualTablet, EvdevVirtualTablet>();
                    this.AddSingleton<IVirtualKeyboard, EvdevVirtualKeyboard>();

                    if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
                    {
                        this.AddSingleton<IVirtualScreen, WaylandDisplay>();
                    }
                    else if (Environment.GetEnvironmentVariable("DISPLAY") != null)
                    {
                        this.AddSingleton<IVirtualScreen, XScreen>();
                    }
                    else
                    {
                        Log.Write("Display", "Neither Wayland nor X11 were detected, defaulting to X11.", LogLevel.Warning);
                        this.AddSingleton<IVirtualScreen, XScreen>();
                    }

                    break;
                }
                case PluginPlatform.MacOS:
                {
                    this.AddTransient<IAbsolutePointer, MacOSAbsolutePointer>();
                    this.AddTransient<IRelativePointer, MacOSRelativePointer>();
                    this.AddTransient<IVirtualKeyboard, MacOSVirtualKeyboard>();
                    this.AddTransient<IVirtualScreen, MacOSDisplay>();

                    goto default;
                }
                default:
                {
                    this.AddTransient<FallbackTimer>();
                    break;
                }
            }
        }

        public DirectoryInfo PluginDirectory { get; }
        protected DirectoryInfo TrashDirectory { get; }
        protected DirectoryInfo TemporaryDirectory { get; }

        protected List<DesktopPluginContext> Plugins { get; } = new List<DesktopPluginContext>();

        public IReadOnlyCollection<DesktopPluginContext> GetLoadedPlugins() => Plugins;

        public event EventHandler AssembliesChanged;

        public void Clean()
        {
            try
            {
                if (PluginDirectory.Exists)
                {
                    foreach (var file in PluginDirectory.GetFiles())
                    {
                        var newPath = Path.Join(PluginDirectory.FullName, file.Name.Replace(file.Extension, string.Empty), file.Name);
                        Directory.CreateDirectory(Directory.GetParent(newPath).FullName);
                        file.MoveTo(newPath);
                    }
                }

                if (TrashDirectory.Exists)
                    Directory.Delete(TrashDirectory.FullName, true);
                if (TemporaryDirectory.Exists)
                    Directory.Delete(TemporaryDirectory.FullName, true);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        public void Load()
        {
            foreach (var dir in PluginDirectory.GetDirectories())
                LoadPlugin(dir);

            AssembliesChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void LoadPlugin(DirectoryInfo directory)
        {
            // "Plugins" are directories that contain managed and unmanaged dll
            // These dlls are loaded into a PluginContext per directory
            directory.Refresh();
            if (Plugins.All(p => p.Directory.Name != directory.Name))
            {
                if (directory.Exists)
                {
                    Log.Write("Plugin", $"Loading plugin '{directory.Name}'", LogLevel.Debug);
                    var context = new DesktopPluginContext(directory);

                    // Populate PluginTypes so desktop implementations can access them
                    ImportTypes(context);
                    Plugins.Add(context);
                }
                else
                {
                    Log.Write("Plugin", $"Tried to load a nonexistent plugin '{directory.Name}'", LogLevel.Warning);
                }
            }
            else
            {
                Log.Write("Plugin", $"Attempted to load the plugin {directory.Name} when it is already loaded.", LogLevel.Debug);
            }
        }

        protected void ImportTypes(PluginContext context)
        {
            var types = from asm in context.Assemblies
                where IsLoadable(asm)
                from type in asm.GetExportedTypes()
                where IsPluginType(type)
                select type;

            types.AsParallel().ForAll(type =>
            {
                if (!IsPlatformSupported(type))
                {
                    Log.Write("Plugin", $"Plugin '{type.FullName}' is not supported on {SystemInterop.CurrentPlatform}", LogLevel.Info);
                    return;
                }

                if (IsPluginIgnored(type))
                {
                    return;
                }

                try
                {
                    if (!Types.Contains(type))
                    {
                        this.AddTransient(type);
                    }
                }
                catch
                {
                    Log.Write("Plugin", $"Plugin '{type.FullName}' incompatible", LogLevel.Warning);
                }
            });
        }

        public bool InstallPlugin(string filePath)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
                return false;

            var name = file.Name.Replace(file.Extension, string.Empty);
            var tempDir = new DirectoryInfo(Path.Join(TemporaryDirectory.FullName, name));
            if (!tempDir.Exists)
                tempDir.Create();

            var pluginPath = Path.Join(AppInfo.Current.PluginDirectory, name);
            var pluginDir = new DirectoryInfo(pluginPath);
            switch (file.Extension)
            {
                case ".zip":
                {
                    ZipFile.ExtractToDirectory(file.FullName, tempDir.FullName, true);
                    break;
                }
                case ".dll":
                {
                    file.CopyTo(Path.Join(tempDir.FullName, file.Name));
                    break;
                }
                default:
                    throw new InvalidOperationException($"Unsupported archive type: {file.Extension}");
            }
            var context = Plugins.FirstOrDefault(ctx => ctx.Directory.FullName == pluginDir.FullName);
            var result = pluginDir.Exists ? UpdatePlugin(context, tempDir) : InstallPlugin(pluginDir, tempDir);

            if (!TemporaryDirectory.GetFileSystemInfos().Any())
                Directory.Delete(TemporaryDirectory.FullName, true);

            if (result)
                LoadPlugin(pluginDir);
            return result;
        }

        public async Task<bool> DownloadPlugin(PluginMetadata metadata)
        {
            string sourcePath = Path.Join(TemporaryDirectory.FullName, metadata.Name);
            string targetPath = Path.Join(PluginDirectory.FullName, metadata.Name);
            string metadataPath = Path.Join(targetPath, "metadata.json");

            var sourceDir = new DirectoryInfo(sourcePath);
            var targetDir = new DirectoryInfo(targetPath);

            await metadata.DownloadAsync(sourcePath);

            var context = Plugins.FirstOrDefault(ctx => ctx.Directory.FullName == targetDir.FullName);
            var result = targetDir.Exists ? UpdatePlugin(context, sourceDir) : InstallPlugin(targetDir, sourceDir);

            using (var fs = File.Create(metadataPath))
                Serialization.Serialize(fs, metadata);

            if (!TemporaryDirectory.GetFileSystemInfos().Any())
                Directory.Delete(TemporaryDirectory.FullName, true);
            return result;
        }

        public bool InstallPlugin(DirectoryInfo target, DirectoryInfo source)
        {
            Log.Write("Plugin", $"Installing plugin '{target.Name}'");
            source.CopyTo(target);
            LoadPlugin(target);
            return true;
        }

        public bool UninstallPlugin(DesktopPluginContext plugin)
        {
            if (plugin == null)
                return false;

            var random = new Random();
            if (!Directory.Exists(TrashDirectory.FullName))
                TrashDirectory.Create();

            Log.Write("Plugin", $"Uninstalling plugin '{plugin.FriendlyName}'");

            var trashPath = Path.Join(TrashDirectory.FullName, $"{plugin.FriendlyName}_{random.Next()}");
            Directory.Move(plugin.Directory.FullName, trashPath);

            return UnloadPlugin(plugin);
        }

        public bool UpdatePlugin(DesktopPluginContext plugin, DirectoryInfo source)
        {
            var targetDir = new DirectoryInfo(plugin.Directory.FullName);
            if (UninstallPlugin(plugin))
                return InstallPlugin(targetDir, source);
            return false;
        }

        public bool UnloadPlugin(DesktopPluginContext context)
        {
            Log.Write("Plugin", $"Unloading plugin '{context.FriendlyName}'", LogLevel.Debug);
            Plugins.Remove(context);
            AssembliesChanged?.Invoke(this, EventArgs.Empty);
            return context.Assemblies.All(p => RemoveAllTypesForAssembly(p));
        }

        public bool RemoveAllTypesForAssembly(Assembly asm)
        {
            try
            {
                var descriptors = from descriptor in this
                    where descriptor.ServiceType.Assembly == asm
                    select descriptor;

                foreach (var descriptor in descriptors)
                    this.Remove(descriptor);

                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return false;
            }
        }
    }
}
