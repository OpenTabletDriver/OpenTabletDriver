using ReactiveUI;
using TabletDriverLib;
using System.Linq;
using TabletDriverPlugin.Attributes;
using System.Reflection;

namespace OpenTabletDriver.Plugins
{
    public class PluginReference : ReactiveObject
    {
        public PluginReference(string path)
        {
            Path = path;
        }

        protected PluginReference()
        {
        }

        private string _name;
        public string Name
        {
            protected set => this.RaiseAndSetIfChanged(ref _name, value);
            get => !string.IsNullOrWhiteSpace(_name) ? _name : Path;
        }

        private string _path;
        public string Path
        {
            protected set
            {
                this.RaiseAndSetIfChanged(ref _path, value);
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

        public override string ToString() => Name;

        public T Construct<T>() where T : class
        {
            return PluginManager.ConstructObject<T>(Path);
        }

        public static readonly PluginReference Disable = new PluginReference
        {
            Name = "{Disable}",
            Path = "{Disable}"
        };
    }
}