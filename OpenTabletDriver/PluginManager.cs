using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver
{
    public static class PluginManager
    {
        private static Collection<TypeInfo> _types;
        public static Collection<TypeInfo> Types
        {
            set => _types = value;
            get => _types ??= new ObservableCollection<TypeInfo>(allTypes.Value);
        }

        private static readonly Assembly pluginAsm = Assembly.GetAssembly(typeof(IDriver));
        private static readonly Type[] pluginAsmTypes = pluginAsm.GetExportedTypes();

        public static bool AddPlugin(FileInfo file)
        {
            if (file.Extension == ".dll" && ImportAssembly(file.FullName) is Assembly asm)
            {
                foreach (var type in GetTypes(asm))
                    Types.Add(type);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static T ConstructObject<T>(string name, object[] args = null) where T : class
        {
            args ??= new object[0];
            if (!string.IsNullOrWhiteSpace(name))
            {
                var type = Types.FirstOrDefault(t => t.FullName == name);
                var matchingConstructors = from ctor in type?.GetConstructors()
                    let parameters = ctor.GetParameters()
                    where parameters.Length == args.Length
                    where ParametersValid(parameters, args)
                    select ctor;
                
                var constructor = matchingConstructors.FirstOrDefault();
                return (T)constructor?.Invoke(args) ?? null;
            }
            else
            {
                return null;
            }
        }

        public static IReadOnlyCollection<TypeInfo> GetChildTypes<T>()
        {
            var children = from type in Types
                where typeof(T).IsAssignableFrom(type)
                select type;
            
            return new List<TypeInfo>(children);
        }

        private static IEnumerable<TypeInfo> GetTypes(Assembly asm)
        {
            try
            {
                return from type in asm.DefinedTypes
                    where TypeIsSupported(type)
                    where TypeImplementsPlugin(type)
                    select type.GetTypeInfo();
            }
            catch (ReflectionTypeLoadException e)
            {
                Log.Write("Plugin", $"Failed to get one or more types. The plugin '{asm.GetName().Name}' is likely out of date.", LogLevel.Error);
                return from type in e.Types.Where(t => t != null)
                    where TypeIsSupported(type)
                    where TypeImplementsPlugin(type)
                    select type.GetTypeInfo();
            }
        }

        private static Assembly ImportAssembly(string path)
        {
            try
            {
                return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            }
            catch (BadImageFormatException)
            {
                Log.Write("Plugin", $"Failed to initialize {path}, incompatible plugin", LogLevel.Warning);
                return null;
            }
        }

        private static bool ParametersValid(ParameterInfo[] parameters, object[] args)
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

        private static bool TypeIsSupported(Type type)
        {
            var attr = (SupportedPlatformAttribute)type.GetCustomAttribute(typeof(SupportedPlatformAttribute), false);
            return attr?.IsCurrentPlatform ?? true;
        }

        private static bool TypeImplementsPlugin(Type type)
        {
            return pluginAsmTypes.Any(t => t.IsAssignableFrom(type) ||
                type.GetInterfaces().Any(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == t));
        }

        private static bool CanLoadAssembly(Assembly asm)
        {
            try
            {
                _ = asm.DefinedTypes;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Lazy<IEnumerable<TypeInfo>> allTypes = new Lazy<IEnumerable<TypeInfo>>(() => 
        {
            return from asm in AppDomain.CurrentDomain.GetAssemblies()
                where CanLoadAssembly(asm)
                from type in GetTypes(asm)
                select type;
        });
    }
}