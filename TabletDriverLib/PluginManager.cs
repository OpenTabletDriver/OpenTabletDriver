using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TabletDriverPlugin;

namespace TabletDriverLib
{
    public static class PluginManager
    {
        public static ObservableCollection<TypeInfo> Types { private set; get; } = GetAllTypes();
        
        public static async Task<bool> AddPlugin(FileInfo file)
        {
            if (file.Extension == ".dll")
            {
                var asm = await ImportAssembly(file.FullName);
                foreach (var type in GetLoadableTypes(asm))
                    Types.Add(type.GetTypeInfo());
                return true;
            }
            else
            {
                return false;
            }
        }

        private static async Task<Assembly> ImportAssembly(string path)
        {
            return await Task.Run<Assembly>(() => Assembly.LoadFile(path));
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Log.Write("Plugin", "Failed to get one or more types. This plugin is likely out of date.", true);
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
        
        private static ObservableCollection<TypeInfo> GetAllTypes()
        {
            var types = Assembly.GetEntryAssembly()
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(x => x.DefinedTypes);
            return new ObservableCollection<TypeInfo>(types);
        }
    }
}