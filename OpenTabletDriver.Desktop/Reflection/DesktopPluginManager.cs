using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class DesktopPluginManager : PluginManager
    {
        public DirectoryInfo PluginDirectory
        {
            get { return field ??= new DirectoryInfo(AppInfo.Current.PluginDirectory); }
            private set;
        }

        protected DirectoryInfo TrashDirectory
        {
            get { return field ??= new DirectoryInfo(AppInfo.Current.TrashDirectory); }
            private set;
        }

        protected DirectoryInfo TemporaryDirectory
        {
            get { return field ??= new DirectoryInfo(AppInfo.Current.TemporaryDirectory); }
            private set;
        }

        protected List<DesktopPluginContext> Plugins { get; } = new List<DesktopPluginContext>();

        public IReadOnlyCollection<DesktopPluginContext> GetLoadedPlugins() => Plugins;

        public event EventHandler? AssembliesChanged;

        /// <summary>
        /// Verify installation of plugins and clean up trash and temporary directories.
        /// This is expected but not always necessary to be run at some point after class initialization but before loading plugins.
        /// </summary>
        public void Clean()
        {
            try
            {
                if (PluginDirectory.Exists)
                {
                    foreach (var file in PluginDirectory.GetFiles())
                    {
                        Log.Write("Plugin", $"Unexpected file found: '{file.FullName}'", LogLevel.Warning);
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
            PluginDirectory = TrashDirectory = TemporaryDirectory = null!; // reset directories

            ResetServices();

            foreach (var dir in PluginDirectory.GetDirectories())
                LoadPlugin(dir);

            AssembliesChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Load a plugin associated with a directory.
        /// </summary>
        /// <param name="directory">The directory containing the plugin files.</param>
        protected void LoadPlugin(DirectoryInfo directory)
        {
            // "Plugins" are directories that contain managed and unmanaged dll
            // These dlls are loaded into a PluginContext per directory
            directory.Refresh();
            if (Plugins.Any(p => p.Directory.Name == directory.Name))
            {
                Log.Write("Plugin", $"Attempted to load the plugin {directory.Name} when it is already loaded.", LogLevel.Debug);
                return;
            }

            Log.Write("Plugin", $"Loading plugin '{directory.Name}'", LogLevel.Debug);

            try
            {
                var context = new DesktopPluginContext(directory);

                // Populate PluginTypes so desktop implementations can access them
                ImportTypes(context);
                Plugins.Add(context);
            }
            catch
            {
                Log.Write("Plugin", $"Failed to completely load plugin '{directory.Name}'. Some problems may occur later. Please double check if this plugin is installed correctly or has any update.", LogLevel.Error, true);
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
                    Log.Write("Plugin", $"Plugin '{type.FullName}' is not supported on {SystemInterop.CurrentPlatform}");
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

        /// <summary>
        /// Install a plugin from a file (.zip or .dll)
        /// </summary>
        /// <param name="filePath">The absolute path to the plugin</param>
        /// <returns>Whether installing the plugin succeeded</returns>
        /// <exception cref="InvalidOperationException">Thrown if the file extension is unsupported (supports <c>.zip</c> and <c>.dll</c>)</exception>
        public bool InstallPlugin(string filePath)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
                return false;

            var name = file.Name.Replace(file.Extension, string.Empty);
            var tempDir = new DirectoryInfo(Path.Join(TemporaryDirectory.FullName, name));

            // Avoid issues with extracting to an existing directory and installing additional unwanted files
            if (tempDir.Exists)
                tempDir.Delete(true);

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

            bool result;
            if (pluginDir.Exists)
            {
                var context = Plugins.First(ctx => ctx.Directory.FullName == pluginDir.FullName);
                result = UpdatePlugin(context, tempDir);
            }
            else
                result = InstallPlugin(pluginDir, tempDir);

            if (!TemporaryDirectory.GetFileSystemInfos().Any())
                Directory.Delete(TemporaryDirectory.FullName, true);

            return result;
        }

        public async Task<bool> DownloadPlugin(PluginMetadata metadata)
        {
            string sourcePath = Path.Join(TemporaryDirectory.FullName, metadata.Name);
            string targetPath = Path.Join(PluginDirectory.FullName, metadata.Name);
            string metadataPath = Path.Join(targetPath, "metadata.json");

            var sourceDir = new DirectoryInfo(sourcePath);
            var targetDir = new DirectoryInfo(targetPath);

            if (sourceDir.Exists)
                sourceDir.Delete(true);

            await metadata.DownloadAsync(sourcePath);

            sourceDir.Refresh();

            bool result;
            if (targetDir.Exists)
            {
                var context = Plugins.First(ctx => ctx.Directory.FullName == targetDir.FullName);
                result = UpdatePlugin(context, sourceDir);
            }
            else
                result = InstallPlugin(targetDir, sourceDir);

            await using (var fs = File.Create(metadataPath))
                Serialization.Serialize(fs, metadata);

            if (!TemporaryDirectory.GetFileSystemInfos().Any())
                Directory.Delete(TemporaryDirectory.FullName, true);
            return result;
        }

        /// <summary>
        /// Install and load plugin from a source folder to a target folder.
        /// </summary>
        /// <param name="target">The folder to install a plugin into.</param>
        /// <param name="source">The folder to install the plugin from.</param>
        /// <returns><c>true</c> if loading the plugin succeeded.</returns>
        public bool InstallPlugin(DirectoryInfo target, DirectoryInfo source)
        {
            Log.Write("Plugin", $"Installing plugin '{target.Name}'");
            source.CopyTo(target);
            LoadPlugin(target);
            return true;
        }

        /// <summary>
        /// Uninstall and unload a plugin by its <see cref="DesktopPluginContext"/>.
        /// </summary>
        /// <param name="plugin">The plugin to uninstall and unload.</param>
        /// <returns><c>true</c> if unloading the plugin succeeded. Uninstalling the files will always succeed (unless an exception is thrown).</returns>
        public bool UninstallPlugin(DesktopPluginContext plugin)
        {
            var random = new Random();
            if (!Directory.Exists(TrashDirectory.FullName))
                TrashDirectory.Create();

            Log.Write("Plugin", $"Uninstalling plugin '{plugin.FriendlyName}'");

            var trashPath = Path.Join(TrashDirectory.FullName, $"{plugin.FriendlyName}_{random.Next()}");
            Directory.Move(plugin.Directory.FullName, trashPath);

            return UnloadPlugin(plugin);
        }

        /// <summary>
        /// Update and load a plugin with contents from a non-plugin directory.
        /// </summary>
        /// <param name="plugin">The plugin context of the old plugin.</param>
        /// <param name="source">The directory info of the new plugin</param>
        /// <returns><c>false</c> if uninstalling or installing the plugin failed, <c>true</c> otherwise.</returns>
        /// <remarks>This does not verify that the new plugin matches the old one.</remarks>
        public bool UpdatePlugin(DesktopPluginContext plugin, DirectoryInfo source)
        {
            var targetDir = new DirectoryInfo(plugin.Directory.FullName);
            if (UninstallPlugin(plugin))
                return InstallPlugin(targetDir, source);
            return false;
        }

        /// <summary>
        /// Unload a currently loaded plugin based on its <see cref="DesktopPluginContext"/>.
        /// </summary>
        /// <param name="context">The currently loaded plugin</param>
        /// <returns><c>true</c> if unloading all relevant assemblies succeeded. If not, you may need to restart the application to unload them entirely.</returns>
        public bool UnloadPlugin(DesktopPluginContext context)
        {
            Log.Write("Plugin", $"Unloading plugin '{context.FriendlyName}'", LogLevel.Debug);
            Plugins.Remove(context);
            AssembliesChanged?.Invoke(this, EventArgs.Empty);
            return context.Assemblies.All(RemoveAllTypesForAssembly);
        }

        public bool RemoveAllTypesForAssembly(Assembly asm)
        {
            try
            {
                var types = pluginTypes.Where(t => t.Assembly == asm)
                    .Select(t => t.GetTypeInfo());

                pluginTypes = new ConcurrentBag<TypeInfo>(pluginTypes.Except(types));
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return false;
            }
        }

        public override void ResetServices()
        {
            base.ResetServices();

            // These services will always be provided on the desktop
            AddService<IServiceProvider>(() => this);
            AddService(() => DesktopInterop.Timer);
            AddService(() => DesktopInterop.AbsolutePointer);
            AddService(() => DesktopInterop.RelativePointer);
            AddService(() => DesktopInterop.VirtualTablet);
            AddService(() => DesktopInterop.VirtualPad);
            AddService(() => DesktopInterop.VirtualScreen);
            AddService(() => DesktopInterop.VirtualKeyboard);
        }
    }
}
