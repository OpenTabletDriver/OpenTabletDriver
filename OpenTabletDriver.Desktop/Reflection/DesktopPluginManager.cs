using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Reflection;

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
        }

        public DirectoryInfo PluginDirectory { get; }
        protected DirectoryInfo TrashDirectory { get; }
        protected DirectoryInfo TemporaryDirectory { get; }

        protected List<DesktopPluginContext> Plugins { get; } = new List<DesktopPluginContext>();

        public IReadOnlyCollection<DesktopPluginContext> GetLoadedPlugins() => Plugins;

        public void Clean()
        {
            try
            {
                foreach (var file in PluginDirectory.GetFiles())
                {
                    var newPath = Path.Join(PluginDirectory.FullName, file.Name.Replace(file.Extension, string.Empty), file.Name);
                    Directory.CreateDirectory(Directory.GetParent(newPath).FullName);
                    file.MoveTo(newPath);
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
        }

        protected void LoadPlugin(DirectoryInfo directory)
        {
            // "Plugins" are directories that contain managed and unmanaged dll
            // These dlls are loaded into a PluginContext per directory
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
                    return;

                try
                {
                    var pluginTypeInfo = type.GetTypeInfo();
                    if (!pluginTypes.Contains(pluginTypeInfo))
                        pluginTypes.Add(pluginTypeInfo);
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
            return result;
        }

        public bool InstallPlugin(DirectoryInfo target, DirectoryInfo source)
        {
            Log.Write("Plugin", $"Installing plugin '{target.Name}'");
            source.MoveTo(target.FullName);
            return true;
        }

        public bool UninstallPlugin(DesktopPluginContext plugin)
        {
            var random = new Random();
            if (!Directory.Exists(TrashDirectory.FullName))
                TrashDirectory.Create();

            Log.Write("Plugin", $"Uninstalling plugin '{plugin.FriendlyName}'");

            var trashPath = Path.Join(TrashDirectory.FullName, $"{plugin.FriendlyName}_{random.Next()}");
            plugin.Directory.MoveTo(trashPath);

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
            return context.Assemblies.All(p => RemoveAllTypesForAssembly(p));
        }

        public bool RemoveAllTypesForAssembly(Assembly asm)
        {
            try
            {
                var types = from type in asm.GetTypes()
                    select type.GetTypeInfo();
                pluginTypes = new ConcurrentBag<TypeInfo>(pluginTypes.Except(types));
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
