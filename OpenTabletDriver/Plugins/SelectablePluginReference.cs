using ReactiveUI;
using TabletDriverLib.Plugins;

namespace OpenTabletDriver.Plugins
{
    public class SelectablePluginReference : PluginReference
    {
        public SelectablePluginReference(string path, bool isEnabled) : base(path)
        {
            IsEnabled = isEnabled;
        }

        public SelectablePluginReference() : base()
        {
            IsEnabled = false;
        }

        public bool IsEnabled { get; set; }
    }
}