using TabletDriverLib;
using System.Linq;
using TabletDriverPlugin.Attributes;
using System.Reflection;

namespace TabletDriverLib.Plugins
{
    public class PluginReference
    {
        public PluginReference(string path)
        {
            Path = path;
        }

        public PluginReference(object obj)
        {
            Path = obj.GetType().FullName;
        }

        protected PluginReference()
        {
        }

        public string Name { private set; get; }

        private string _path;
        public string Path
        {
            protected set
            {
                _path = value;
                Name = GetName(Path);
            }
            get => _path;
        }

        internal static string GetName(string path)
        {
            if (PluginManager.Types.FirstOrDefault(t => t.FullName == path) is TypeInfo plugin)
            {
                var attrs = plugin.GetCustomAttributes(false);
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

        public TypeInfo GetTypeReference<T>()
        {
            var types = from type in PluginManager.GetChildTypes<T>()
                where type.FullName == Path
                select type;
            
            return types.FirstOrDefault();
        }

        public static readonly PluginReference Disable = new PluginReference
        {
            Name = "{Disable}",
            Path = "{Disable}"
        };
    }
}