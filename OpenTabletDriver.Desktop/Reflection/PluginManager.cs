using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginManager
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public PluginManager()
        {
            Type[] internalTypes;
            try
            {
                internalTypes = AssemblyLoadContext.Default.Assemblies
                    .Where(asm => asm != pluginAsm)
                    .SelectMany(asm => asm.Modules)
                    .Where(module => module.ScopeName.Contains("OpenTabletDriver"))
                    .SelectMany(module => module.FindTypes(internalPluginFilter, null))
                    .ToArray();
            }
            catch (ReflectionTypeLoadException e)
            {
                internalTypes = e.Types.Where(t => t != null).ToArray();
            }

            pluginTypes = new ConcurrentBag<Type>(internalTypes);
        }

        public IReadOnlyCollection<Type> PluginTypes => pluginTypes;
        protected ConcurrentBag<Type> pluginTypes;

        private readonly static Assembly pluginAsm = Assembly.GetAssembly(typeof(IDriver));
        private readonly static Type[] libTypes = pluginAsm.Modules
            .Where(module => module.ScopeName.Contains("OpenTabletDriver"))
            .SelectMany(module => module.FindTypes(libTypeFilter, null)).ToArray();

        public PluginReference GetPluginReference(string path) => new PluginReference(this, path);
        public PluginReference GetPluginReference(Type type) => GetPluginReference(type.FullName);
        public PluginReference GetPluginReference(object obj) => GetPluginReference(obj.GetType());

        public T ConstructObject<T>(string name, object[] args = null) where T : class
        {
            args ??= Array.Empty<object>();
            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    if (PluginTypes.FirstOrDefault(t => t.FullName == name) is Type type)
                    {
                        var matchingConstructors = from ctor in type.GetConstructors()
                        let parameters = ctor.GetParameters()
                        where parameters.Length == args.Length
                        where IsValidParameterFor(args, parameters)
                        select ctor;

                        if (matchingConstructors.FirstOrDefault() is ConstructorInfo constructor)
                            return (T)constructor.Invoke(args) ?? null;
                    }
                }
                catch
                {
                    Log.Write("Plugin", $"Unable to construct object '{name}'", LogLevel.Error);
                }
            }
            Log.Write("Plugin", $"No constructor found for '{name}'", LogLevel.Debug);
            return null;
        }

        public IReadOnlyCollection<Type> GetChildTypes<T>()
        {
            var children = from type in PluginTypes
                where typeof(T).IsAssignableFrom(type)
                select type;

            return children.ToArray();
        }

        protected static bool IsValidParameterFor(object[] args, ParameterInfo[] parameters)
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

        protected static bool IsPluginType(Type type)
        {
            return type.IsPublic & !type.IsAbstract & !type.IsInterface &&
                libTypes.Any(t => t.IsAssignableFrom(type) ||
                    type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == t));
        }

        protected static bool IsPlatformSupported(Type type)
        {
            var attr = type.GetCustomAttribute<SupportedPlatformAttribute>(false);
            return attr?.IsCurrentPlatform ?? true;
        }

        protected static bool IsPluginIgnored(Type type)
        {
            return type.GetCustomAttribute<PluginIgnoreAttribute>(false) is PluginIgnoreAttribute;
        }

        private static bool libTypeFilter(Type type, object _)
        {
            return type.IsAbstract | type.IsInterface;
        }

        private bool internalPluginFilter(Type type, object _)
        {
            return IsPluginType(type) && IsPlatformSupported(type);
        }
    }
}
