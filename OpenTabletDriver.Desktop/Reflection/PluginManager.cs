using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginManager : ServiceCollection
    {
        public PluginManager()
        {
            var internalTypes = from asm in AssemblyLoadContext.Default.Assemblies
                where IsLoadable(asm)
                from type in asm.DefinedTypes
                where type.IsPublic && !(type.IsInterface || type.IsAbstract)
                where IsPluginType(type)
                where IsPlatformSupported(type)
                select type;

            foreach (var type in internalTypes)
            {
                this.AddTransient(type);
            }
        }

        public IEnumerable<Type> Types => from descriptor in this
            where descriptor.ServiceType != null
            select descriptor.ServiceType;

        protected readonly static IEnumerable<Type> libTypes =
            from type in Assembly.GetAssembly(typeof(IDriver)).GetExportedTypes()
                where type.IsAbstract || type.IsInterface
                select type;

        public virtual IReadOnlyCollection<Type> GetChildTypes<T>()
        {
            var children = from type in Types
                where typeof(T).IsAssignableFrom(type)
                select type;

            return children.ToArray();
        }

        public virtual Type GetTypeFromPath(string path)
        {
            return Types.FirstOrDefault(t => t.FullName == path);
        }

        public virtual string GetFriendlyName(string path)
        {
            if (GetTypeFromPath(path) is TypeInfo plugin)
            {
                return plugin.GetFriendlyName();
            }
            return null;
        }

        protected virtual bool IsValidParameterFor(object[] args, ParameterInfo[] parameters)
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

        protected virtual bool IsPluginType(Type type)
        {
            return !type.IsAbstract && !type.IsInterface &&
                libTypes.Any(t => t.IsAssignableFrom(type) ||
                type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == t));
        }

        protected virtual bool IsPlatformSupported(Type type)
        {
            var attr = (SupportedPlatformAttribute)type.GetCustomAttribute(typeof(SupportedPlatformAttribute), false);
            return attr?.IsCurrentPlatform ?? true;
        }

        protected virtual bool IsPluginIgnored(Type type)
        {
            return type.GetCustomAttributes(false).Any(a => a.GetType() == typeof(PluginIgnoreAttribute));
        }

        protected virtual bool IsLoadable(Assembly asm)
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
