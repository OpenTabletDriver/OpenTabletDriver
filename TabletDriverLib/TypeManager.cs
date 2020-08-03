using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using NativeLib;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;

namespace TabletDriverLib
{
    public static class TypeManager
    {
        private static Collection<TypeInfo> _types;
        public static Collection<TypeInfo> Types
        {
            set => _types = value;
            get => _types ??= allTypes.Value;
        }

        private static Collection<FileInfo> LoadedFiles { set; get; } = new Collection<FileInfo>();

        public static event Action<Assembly> AssemblyLoaded;
        
        public static bool AddPlugin(FileInfo file)
        {
            if (file.Extension == ".dll" && !LoadedFiles.Contains(file))
            {
                var asm = ImportAssembly(file.FullName);
                foreach (var type in GetLoadableTypes(asm))
                {
                    var attr = type.GetCustomAttribute(typeof(SupportedPlatformAttribute), true) as SupportedPlatformAttribute;
                    if (attr == null || attr.IsCurrentPlatform)
                        Types.Add(type.GetTypeInfo());
                }
                AssemblyLoaded?.Invoke(asm);
                LoadedFiles.Add(file);
                return true;
            }
            else
            {
                return false;
            }
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
                    select type;
                    
                return new ObservableCollection<TypeInfo>(types);
            }
            catch (ReflectionTypeLoadException)
            {
                var types = from assembly in Assembly.GetEntryAssembly().GetReferencedAssemblies()
                    let loadedAsm = Assembly.Load(assembly)
                    from type in loadedAsm.DefinedTypes
                    select type;

                return new ObservableCollection<TypeInfo>(types);
            }
        });
    }
}