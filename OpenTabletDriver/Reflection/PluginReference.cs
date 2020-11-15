using System;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.Reflection
{
    public class PluginReference : IEquatable<PluginReference>
    {
        protected PluginReference()
        {
        }

        public PluginReference(string path) : this()
        {
            Path = path;
            Name = GetName(path);
        }

        public PluginReference(object obj) : this(obj.GetType().FullName)
        {
        }

        public PluginReference(Type t) : this(t.FullName)
        {
        }

        public string Name { private set; get; }
        public string Path { private set; get; }

        internal static string GetName(string path)
        {
            if (PluginManager.Types.FirstOrDefault(t => t.FullName == path) is TypeInfo plugin)
            {
                var attrs = plugin.GetCustomAttributes(true);
                var nameattr = attrs.FirstOrDefault(t => t.GetType() == typeof(PluginNameAttribute));
                if (nameattr is PluginNameAttribute attr)
                    return attr.Name;
            }
            return null;
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? Path : Name;

        public T Construct<T>() where T : class
        {
            return PluginManager.ConstructObject<T>(Path);
        }

        public T Construct<T>(params object[] args) where T : class
        {
            return PluginManager.ConstructObject<T>(Path, args);
        }

        public TypeInfo GetTypeReference<T>()
        {
            var types = from type in PluginManager.GetChildTypes<T>()
                where type.FullName == Path
                select type;
            
            return types.FirstOrDefault();
        }

        public bool Equals(PluginReference other)
        {
            return Name == other.Name && Path == other.Path;
        }
    }
}