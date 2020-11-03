using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver
{
    public static class PluginManager
    {
        public static IReadOnlyCollection<TypeInfo> PluginTypes => pluginTypes;
        private static bool Silent;

        public static async Task LoadPluginsAsync(bool silent = false)
        {
            await Task.Run(() => LoadPlugins(silent));
        }

        public static void LoadPlugins(bool silent)
        {
            Silent = silent;
            static void loadPlugin(PluginContext context, string plugin)
            {
                try
                {
                    context.LoadFromAssemblyPath(plugin);
                }
                catch
                {
                    var pluginFile = new FileInfo(plugin);
                    Log($"Failed loading assembly '{pluginFile.Name}'", LogLevel.Error);
                }
            }

            pluginTypes.Clear();

            (from asm in AssemblyLoadContext.Default.Assemblies
                from type in asm.DefinedTypes
                where type.IsPublic && type.IsPluginType()
                select type)
            .AsParallel().ForAll(t => pluginTypes.Add(t.GetTypeInfo()));

            // "Plugins" are directories that contain managed and unmanaged dll
            //  These dlls are loaded into a PluginContext per directory

            Parallel.ForEach(Directory.GetDirectories(AppInfo.Current.PluginDirectory), (dir, state, index) =>
            {
                var pluginName = new DirectoryInfo(dir).Name;
                if (plugins.Any((p) => p.PluginName == pluginName))
                {
                    return;
                }

                Log($"Loading plugin '{pluginName}'");
                var context = new PluginContext(pluginName);
                foreach(var plugin in Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories))
                {
                    loadPlugin(context, plugin);
                }

                plugins.Add(context);
            });

            // If there are plugins found outside subdirectories then load into FallbackPluginContext
            // This fallback does not support loading unmanaged dll if the default loader fails
            // We don't worry with duplicate entries here since CLR won't load duplicate assemblies of the same file

            foreach(var plugin in Directory.EnumerateFiles(AppInfo.Current.PluginDirectory))
            {
                var pluginFile = new FileInfo(plugin);
                Log($"Loading deprecated plugin '{plugin}'");
                loadPlugin(fallbackPluginContext, plugin);
            }

            // Populate PluginTypes so UX and Daemon can access them

            Parallel.ForEach(plugins, (context, _, index) =>
            {
                LoadPluginTypes(context);
            });
            LoadPluginTypes(fallbackPluginContext);
        }

        private static void LoadPluginTypes(PluginContext context)
        {
            (from asm in context.Assemblies
                from type in asm.GetExportedTypes()
                where type.IsPluginType()
                select type)
            .AsParallel().ForAll(type =>
            {
                if (!type.IsPlatformSupported())
                {
                    Log($"Plugin '{type.FullName}' incompatible to current OS", LogLevel.Debug);
                    return;
                }
                if (!type.IsPluginIgnored())
                {
                    Log($"Plugin '{type.FullName}' ignored", LogLevel.Debug);
                    return;
                }
                try
                {
                    var pluginTypeInfo = type.GetTypeInfo();
                    if (!pluginTypes.Any((t) => t == pluginTypeInfo))
                        pluginTypes.Add(pluginTypeInfo);
                }
                catch
                {
                    Log($"Plugin '{type.FullName}' incompatible", LogLevel.Debug);
                }
            });
        }

        public static T ConstructObject<T>(string name, object[] args = null) where T : class
        {
            static bool parametersValid(ParameterInfo[] parameters, object[] args)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var arg = args[i];
                    if (!parameter.ParameterType.IsAssignableFrom(arg.GetType()))
                        return false;
                }
                return true;
            }

            args ??= new object[0];
            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    var type = PluginTypes.FirstOrDefault(t => t.FullName == name);
                    var matchingConstructors = from ctor in type?.GetConstructors()
                                               let parameters = ctor.GetParameters()
                                               where parameters.Length == args.Length
                                               where parametersValid(parameters, args)
                                               select ctor;

                    var constructor = matchingConstructors.FirstOrDefault();
                    return (T)constructor?.Invoke(args) ?? null;
                }
                catch
                {
                    Log($"Unable to construct object '{name}'", LogLevel.Error);
                }
            }
            return null;
        }

        public static IReadOnlyCollection<TypeInfo> GetChildTypes<T>()
        {
            var children = from type in PluginTypes
                           where typeof(T).IsAssignableFrom(type)
                           select type;

            return children.ToArray();
        }

        private static bool IsPluginType(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface &&
                libTypes.Any(t => t.IsAssignableFrom(type) || type.GetInterfaces().Any(
                x => x.IsGenericType && x.GetGenericTypeDefinition() == t));
        }

        private static bool IsPlatformSupported(this Type type)
        {
            var attr = (SupportedPlatformAttribute)type.GetCustomAttribute(typeof(SupportedPlatformAttribute), false);
            return attr?.IsCurrentPlatform ?? true;
        }

        private static void Log(string msg, LogLevel level = LogLevel.Info)
        {
            if (!Silent)
                Plugin.Log.Write("Plugin", msg, level);
        }

        private readonly static PluginContext fallbackPluginContext = new PluginContext();
        private readonly static List<PluginContext> plugins = new List<PluginContext>();
        private readonly static ConcurrentBag<TypeInfo> pluginTypes = new ConcurrentBag<TypeInfo>();
        private readonly static IReadOnlyCollection<Type> libTypes =
            Assembly.GetAssembly(typeof(IDriver)).GetExportedTypes().ToArray();
    }
}