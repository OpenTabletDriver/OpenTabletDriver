using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenTabletDriver.Native;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class DesktopPluginManager : PluginManager
    {
        public DesktopPluginManager(DirectoryInfo directory)
        {
            FallbackPluginContext = new DesktopPluginContext(directory);
        }

        public DesktopPluginManager(string directoryPath)
            : this(new DirectoryInfo(directoryPath))
        {
        }

        protected PluginContext FallbackPluginContext { get; }

        public void LoadPlugins(DirectoryInfo directory)
        {
            pluginTypes.Clear();

            var internalTypes = from asm in AssemblyLoadContext.Default.Assemblies
                from type in asm.DefinedTypes
                where type.IsPublic && !(type.IsInterface || type.IsAbstract)
                where IsPluginType(type)
                where IsPlatformSupported(type)
                select type;

            internalTypes.AsParallel().ForAll(t => pluginTypes.Add(t.GetTypeInfo()));

            // "Plugins" are directories that contain managed and unmanaged dll
            //  These dlls are loaded into a PluginContext per directory
            Parallel.ForEach(directory.GetDirectories(), (dir, state, index) =>
            {
                Log.Write("Plugin", $"Loading plugin '{dir.Name}'");
                var context = new DesktopPluginContext(dir);
                foreach (var plugin in Directory.EnumerateFiles(dir.FullName, "*.dll"))
                    LoadPlugin(context, plugin);

                plugins.Add(context);
            });

            // If there are plugins found outside subdirectories then load into FallbackPluginContext
            // This fallback does not support loading unmanaged dll if the default loader fails
            // We don't worry with duplicate entries here since CLR won't load duplicate assemblies of the same file
            foreach (var plugin in directory.EnumerateFiles("*.dll"))
            {
                var name = Regex.Match(plugin.Name, $"^(.+?){plugin.Extension}").Groups[1].Value;
                Log.Write("Plugin", $"Loading independent plugin '{name}'");
                LoadPlugin(FallbackPluginContext, plugin.FullName);
            }

            // Populate PluginTypes so UX and Daemon can access them
            Parallel.ForEach(plugins, (loadedContext, _, index) =>
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
                    Log.Write("Plugin", $"Plugin '{type.FullName}' is not supported on {SystemInfo.CurrentPlatform}", LogLevel.Info);
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

        protected static bool IsLoadable(Assembly asm)
        {
            try
            {
                _ = asm.DefinedTypes;
                return true;
            }
            catch
            {
                var asmName = asm.GetName();
                Log.Write("Plugin", $"Plugin '{asmName.Name}, Version={asmName.Version}' can't be loaded and is likely out of date.", LogLevel.Warning);
                return false;
            }
        }
    }
}
