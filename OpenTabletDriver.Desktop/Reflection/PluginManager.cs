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
            var internalTypes = AssemblyLoadContext.Default.Assemblies
                .Where(asm => asm != pluginAsm)
                .SelectMany(asm => SafeGetTypes(asm))
                .Where(type => type.IsPublic && !(type.IsInterface || type.IsAbstract))
                .Where(type => IsPluginType(type) && IsPlatformSupported(type));

            pluginTypes = new ConcurrentBag<Type>(internalTypes);
        }

        public IReadOnlyCollection<Type> PluginTypes => pluginTypes;
        protected ConcurrentBag<Type> pluginTypes;

        protected readonly static Assembly pluginAsm = Assembly.GetAssembly(typeof(IDriver));
        protected readonly static Type[] libTypes = pluginAsm.ExportedTypes
            .Where(type => type.IsAbstract | type.IsInterface)
            .ToArray();

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
                    if (PluginTypes.FirstOrDefault(t => t.FullName == name) is TypeInfo type)
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

        protected bool IsValidParameterFor(object[] args, ParameterInfo[] parameters)
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

        protected bool IsPluginType(Type type)
        {
            return !type.IsAbstract & !type.IsInterface &&
                libTypes.Any(t => t.IsAssignableFrom(type) ||
                    type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == t));
        }

        protected bool IsPlatformSupported(Type type)
        {
            var attr = type.GetCustomAttribute<SupportedPlatformAttribute>(false);
            return attr?.IsCurrentPlatform ?? true;
        }

        protected bool IsPluginIgnored(Type type)
        {
            return type.GetCustomAttribute<PluginIgnoreAttribute>(false) is PluginIgnoreAttribute;
        }

        protected IEnumerable<Type> SafeGetTypes(Assembly asm)
        {
            try
            {
                return asm.ExportedTypes;
            }
            catch
            {
                var asmName = asm.GetName();
                Log.Write("Plugin", $"Plugin '{asmName.Name}, Version={asmName.Version}' can't be loaded and is likely out of date.", LogLevel.Warning);
                return Array.Empty<Type>();
            }
        }
    }
}
