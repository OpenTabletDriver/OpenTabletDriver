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
        private readonly static PluginContext fallbackPluginContext = new PluginContext();
        private readonly static ConcurrentBag<PluginContext> plugins = new ConcurrentBag<PluginContext>();
        private readonly static ConcurrentBag<TypeInfo> pluginTypes = new ConcurrentBag<TypeInfo>();
        private readonly static IEnumerable<Type> libTypes = from type in Assembly.GetAssembly(typeof(IDriver)).GetExportedTypes()
                                                             where type.IsAbstract || type.IsInterface
                                                             select type;

        public static async Task LoadPluginsAsync(bool silent = false)
        {
            await Task.Run(() => LoadPlugins(silent));
        }

        public static void LoadPlugins(bool silent)
        {
            Silent = silent;

            pluginTypes.Clear();

            var internalTypes = from asm in AssemblyLoadContext.Default.Assemblies
                                from type in asm.DefinedTypes
                                where type.IsPublic && !(type.IsInterface || type.IsAbstract)
                                where type.IsPluginType()
                                select type;

            internalTypes.AsParallel().ForAll(t => pluginTypes.Add(t.GetTypeInfo()));

            // "Plugins" are directories that contain managed and unmanaged dll
            //  These dlls are loaded into a PluginContext per directory

            Parallel.ForEach(Directory.GetDirectories(AppInfo.Current.PluginDirectory), (dir, state, index) =>
            {
                var pluginName = new DirectoryInfo(dir).Name;
                if (plugins.Any((p) => p.PluginName == pluginName))
                    return;

                Log($"Loading plugin '{pluginName}'");
                var context = new PluginContext(pluginName);
                foreach(var plugin in Directory.EnumerateFiles(dir, "*.dll"))
                    LoadPlugin(context, plugin);

                plugins.Add(context);
            });

            // If there are plugins found outside subdirectories then load into FallbackPluginContext
            // This fallback does not support loading unmanaged dll if the default loader fails
            // We don't worry with duplicate entries here since CLR won't load duplicate assemblies of the same file

            foreach(var plugin in Directory.EnumerateFiles(AppInfo.Current.PluginDirectory, "*.dll"))
            {
                var pluginFile = new FileInfo(plugin);
                Log($"Loading deprecated plugin '{plugin}'");
                LoadPlugin(fallbackPluginContext, plugin);
            }

            // Populate PluginTypes so UX and Daemon can access them

            Parallel.ForEach(plugins, (loadedContext, _, index) =>
            {
                LoadPluginTypes(loadedContext);
            });
            LoadPluginTypes(fallbackPluginContext);
        }

        private static void LoadPlugin(PluginContext context, string plugin)
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

        private static void LoadPluginTypes(PluginContext context)
        {
            var types = from asm in context.Assemblies
                        where asm.IsLoadable()
                        from type in asm.GetExportedTypes()
                        where type.IsPluginType()
                        select type;

            types.AsParallel().ForAll(type =>
            {
                if (!type.IsPlatformSupported())
                {
                    Log($"Plugin '{type.FullName}' incompatible with current OS", LogLevel.Warning);
                    return;
                }
                if (type.IsPluginIgnored())
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
                    Log($"Plugin '{type.FullName}' incompatible", LogLevel.Warning);
                }
            });
        }

        public static T ConstructObject<T>(string name, object[] args = null) where T : class
        {
            args ??= new object[0];
            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    var type = PluginTypes.FirstOrDefault(t => t.FullName == name);
                    var matchingConstructors = from ctor in type?.GetConstructors()
                                               let parameters = ctor.GetParameters()
                                               where parameters.Length == args.Length
                                               where args.IsValidParameterFor(parameters)
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

        private static bool IsValidParameterFor(this object[] args, ParameterInfo[] parameters)
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

        private static bool IsPluginType(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface &&
                libTypes.Any(t => t.IsAssignableFrom(type) ||
                    type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == t));
        }

        private static bool IsPlatformSupported(this Type type)
        {
            var attr = (SupportedPlatformAttribute)type.GetCustomAttribute(typeof(SupportedPlatformAttribute), false);
            return attr?.IsCurrentPlatform ?? true;
        }

        private static bool IsLoadable(this Assembly asm)
        {
            try
            {
                _ = asm.DefinedTypes;
                return true;
            }
            catch
            {
                Log($"Plugin '{asm.GetName().Name}' can't be loaded and is likely out of date.");
                return false;
            }
        }

        public static void Log(string msg, LogLevel level = LogLevel.Info)
        {
            if (!Silent)
                Plugin.Log.Write("Plugin", msg, level);
        }
    }
}