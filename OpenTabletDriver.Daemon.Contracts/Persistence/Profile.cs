using System.Collections.ObjectModel;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class Profile
    {
        public string Tablet { get; set; } = string.Empty;
        public int PersistentId { get; set; }
        public PluginSettings OutputMode { get; set; } = new PluginSettings();
        public Collection<PluginSettings> Filters { get; set; } = new Collection<PluginSettings>();
        public BindingSettings Bindings { get; set; } = new BindingSettings();
    }
}
