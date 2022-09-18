using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    [JsonObject]
    internal class PluginSettingStore : IMigrate<PluginSettings>
    {
        public string? Path { set; get; }
        public Collection<PluginSetting>? Settings { set; get; }
        public bool Enable { set; get; }

        public PluginSettings? Migrate(IServiceProvider serviceProvider)
        {
            if (Path != null)
                Path = MigrationUtils.MigrateNamespace(Path);

            return this.SerializeMigrate<PluginSettingStore, PluginSettings>();
        }
    }
}
