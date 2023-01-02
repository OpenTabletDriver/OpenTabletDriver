using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    [JsonObject]
    internal class PluginSetting : IMigrate<Reflection.PluginSetting>
    {
        public string? Property { set; get; }
        public JToken? Value { set; get; }

        public Reflection.PluginSetting? Migrate(IServiceProvider serviceProvider)
        {
            return this.SerializeMigrate<PluginSetting, Reflection.PluginSetting>();
        }
    }
}
