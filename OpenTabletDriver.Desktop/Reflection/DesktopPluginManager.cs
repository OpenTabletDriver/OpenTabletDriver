using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class DesktopPluginManager : PluginManager
    {
        public DesktopPluginManager(DirectoryInfo directory) : base()
        {
            FallbackPluginContext = new DesktopPluginContext(directory);
            PluginStateManager = new PluginStateManager(this);
            PluginStateManager.ProcessPendingStates();
        }

        public DesktopPluginManager(string directoryPath)
            : this(new DirectoryInfo(directoryPath))
        {
        }

        protected PluginStateManager PluginStateManager { get; }
        protected List<FileInfo> IndependentPlugins { get; } = new List<FileInfo>();
        protected DesktopPluginContext FallbackPluginContext { get; }
        protected ConcurrentBag<DesktopPluginContext> PluginContexts { get; } = new ConcurrentBag<DesktopPluginContext>();
        protected ConcurrentBag<PluginInfo> PluginInfos { get; } = new ConcurrentBag<PluginInfo>();

        public IEnumerable<PluginInfo> GetLoadedPluginInfos()
        {
            return PluginInfos;
        }

        public void LoadPlugins(DirectoryInfo directory)
        {
            // "Plugins" are directories that contain managed and unmanaged dll
            //  These dlls are loaded into a PluginContext per directory
            Parallel.ForEach(directory.GetDirectories(), (dir, state, index) =>
            {
                if (PluginContexts.All(p => p.PluginDirectory.Name != dir.Name))
                {
                    Log.Write("Plugin", $"Loading plugin '{dir.Name}'");
                    var context = new DesktopPluginContext(dir);
                    foreach (var plugin in Directory.EnumerateFiles(dir.FullName, "*.dll"))
                        LoadPlugin(context, plugin);

                    var pluginInfo = new PluginInfo
                    {
                        Name = dir.Name,
                        Path = dir.FullName,
                        Form = PluginForm.Directory
                    };

                    PluginInfos.Add(pluginInfo);
                    PluginContexts.Add(context);
                }
            });

            // If there are plugins found outside subdirectories then load into FallbackPluginContext
            // This fallback does not support loading unmanaged dll if the default loader fails
            foreach (var plugin in directory.EnumerateFiles("*.dll"))
            {
                if (IndependentPlugins.All(f => f.Name != plugin.Name))
                {
                    var name = Path.GetFileNameWithoutExtension(plugin.Name);
                    Log.Write("Plugin", $"Loading independent plugin '{name}'");
                    LoadPlugin(FallbackPluginContext, plugin.FullName);

                    var pluginInfo = new PluginInfo
                    {
                        Name = name,
                        Path = plugin.FullName,
                        Form = PluginForm.File
                    };

                    PluginInfos.Add(pluginInfo);
                    IndependentPlugins.Add(plugin);
                }
            }

            // Populate PluginTypes so UX and Daemon can access them
            Parallel.ForEach(PluginContexts, (loadedContext, _, index) =>
            {
                LoadPluginTypes(loadedContext);
            });
            LoadPluginTypes(FallbackPluginContext);
        }

        protected void LoadPlugin(PluginContext context, string plugin)
        {
            try
            {
                context.LoadFromAssemblyPath(plugin);
            }
            catch
            {
                var pluginFile = new FileInfo(plugin);
                Log.Write("Plugin", $"Failed loading assembly '{pluginFile.Name}'", LogLevel.Error);
            }
        }

        protected void LoadPluginTypes(PluginContext context)
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
                if (type.IsPluginIgnored())
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

        public PluginStateResult InstallPlugin(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (PluginInfos.Where(p => p.State == PluginState.PendingUninstall || p.State == PluginState.PendingUpdate)
                .Any(p => p.Name == Path.GetFileNameWithoutExtension(filePath)))
            {
                return PluginStateResult.AlreadyQueued;
            }

            switch (fileInfo.Extension)
            {
                case ".zip":
                {
                    var path = Path.Join(AppInfo.Current.PluginDirectory, fileInfo.Name.Replace(".zip", string.Empty));
                    Log.Write("Plugin", $"Installing plugin zip: '{fileInfo.Name}'");
                    if (Directory.Exists(path))
                    {
                        return PluginStateManager.QueueUpdate(filePath);
                    }
                    else
                    {
                        ZipFile.ExtractToDirectory(filePath, path, true);
                        return PluginStateResult.Installed;
                    }
                }
                case ".dll":
                {
                    Log.Write("Plugin", $"Installing plugin dll: '{fileInfo.Name}'");
                    if (File.Exists(Path.Join(AppInfo.Current.PluginDirectory, filePath)))
                    {
                        return PluginStateManager.QueueUpdate(filePath);
                    }
                    else
                    {
                        var name = Path.GetFileName(filePath);
                        var dest = Path.Join(AppInfo.Current.PluginDirectory, name);
                        File.Copy(filePath, dest, true);
                        return PluginStateResult.Installed;
                    }
                }
                default:
                    return PluginStateResult.Error;
            }
        }

        public PluginStateResult UninstallPlugin(PluginInfo plugin)
        {
            return PluginStateManager.QueueUninstall(plugin);
        }
    }
}
