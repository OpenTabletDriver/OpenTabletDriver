using System;
using System.Linq;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginReference : IEquatable<PluginReference>
    {
        public PluginReference(PluginManager pluginManager, string path)
        {
            PluginManager = pluginManager;
            Path = path;
            Name = GetName(path);
        }

        public PluginReference(PluginManager pluginManager, Type type)
            : this(pluginManager, type.FullName)
        {
        }
        
        public PluginManager PluginManager { get; }
        public string Name { get; }
        public string Path { get; }

        protected string GetName(string path)
        {
            if (PluginManager.PluginTypes.FirstOrDefault(t => t.FullName == path) is Type plugin)
            {
                var attrs = plugin.GetCustomAttributes(true);
                var nameattr = attrs.FirstOrDefault(t => t.GetType() == typeof(PluginNameAttribute));
                if (nameattr is PluginNameAttribute attr)
                    return attr.Name;
            }
            return null;
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? Path : Name;

        public virtual T Construct<T>(params object[] args) where T : class
        {
            return PluginManager.ConstructObject<T>(Path, args);
        }

        public Type GetTypeReference<T>()
        {
            return PluginManager.GetChildTypes<T>().FirstOrDefault(t => t.FullName == this.Path);
        }

        public Type GetTypeReference()
        {
            return PluginManager.PluginTypes.FirstOrDefault(t => t.FullName == this.Path);
        }

        public bool Equals(PluginReference other) => this.Path == other.Path;
    }
}
