using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class PluginDto
    {
        public PluginDto(string path, ImmutableArray<string> pluginInterfaces, string name, ImmutableArray<PluginSettingMetadata> settingsMetadata, ReadOnlyDictionary<string, string> attributes)
        {
            Path = path;
            PluginInterfaces = pluginInterfaces;
            Name = name;
            SettingsMetadata = settingsMetadata;
            Attributes = attributes;
        }

        public string Path { get; }
        public ImmutableArray<string> PluginInterfaces { get; }
        public string Name { get; }
        public ImmutableArray<PluginSettingMetadata> SettingsMetadata { get; }
        public ReadOnlyDictionary<string, string> Attributes { get; }

        public override string ToString() => Name;
    }
}
