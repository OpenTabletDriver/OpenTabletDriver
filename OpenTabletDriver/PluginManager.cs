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
            get => _types ??= allTypes.Value;
        }
        
        public static bool AddPlugin(FileInfo file)
        {
            if (file.Extension == ".dll")
            {
                var asm = ImportAssembly(file.FullName);
                foreach (var type in GetLoadableTypes(asm))
                {
                    if (TypeIsSupported(type))
                        Types.Add(type.GetTypeInfo());
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool TypeIsSupported(Type type)
        {
            var attr = (SupportedPlatformAttribute)type.GetCustomAttribute(typeof(SupportedPlatformAttribute), false);
            return attr?.IsCurrentPlatform ?? true;
        }

        private static Assembly ImportAssembly(string path)
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Log.Write("Plugin", $"Failed to get one or more types. The plugin '{asm.GetName().Name}' is likely out of date.", LogLevel.Error);
                return e.Types.Where(t => t != null);
            }
        }

        public static T ConstructObject<T>(string name) where T : class
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var type = Types.FirstOrDefault(t => t.FullName == name);
                var ctor = type?.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
                var parameters = new object[] {};
                return (T)ctor?.Invoke(parameters) ?? null;
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

        private static Lazy<Collection<TypeInfo>> allTypes = new Lazy<Collection<TypeInfo>>(() => 
        {
            try
            {
                var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.DefinedTypes
                    where TypeIsSupported(type)
                    select type;
                    
                return new ObservableCollection<TypeInfo>(types);
            }
            catch (ReflectionTypeLoadException)
            {
                var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    where CanLoadAssembly(assembly)
                    from type in assembly.DefinedTypes
                    where TypeIsSupported(type)
                    select type;

                return new ObservableCollection<TypeInfo>(types);
            }
        });

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
    }
}