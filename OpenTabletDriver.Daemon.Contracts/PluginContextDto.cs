using System.Collections.ObjectModel;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class PluginContextDto
    {
        public PluginMetadata Metadata { get; set; } = new();
        public Collection<PluginDto> Plugins { get; set; } = new();
    }
}
