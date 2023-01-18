using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    internal class Settings : IMigrate<Desktop.Settings>
    {
        [JsonProperty("Profiles")]
        public Collection<Profile>? Profiles { set; get; }

        [JsonProperty("LockUsableAreaDisplay")]
        public bool LockUsableAreaDisplay { set; get; }

        [JsonProperty("LockUsableAreaTablet")]
        public bool LockUsableAreaTablet { set; get; }

        [JsonProperty("Tools")]
        public Collection<PluginSettingStore>? Tools { set; get; }

        public Desktop.Settings Migrate(IServiceProvider serviceProvider)
        {
            var pluginFactory = serviceProvider.GetRequiredService<IPluginFactory>();

            var profilesQuery = Profiles?.MigrateAll<Profile, Profiles.Profile>(serviceProvider);
            var tools = Tools?.MigrateAll<PluginSettingStore, PluginSettings>(serviceProvider);

            var profiles = new List<Profiles.Profile>(profilesQuery ?? Array.Empty<Profiles.Profile>());
            foreach (var profile in profiles)
            {
                var outputModeType = pluginFactory.GetPluginType(profile.OutputMode.Path);
                if (outputModeType?.IsAssignableTo(typeof(AbsoluteOutputMode)) ?? false)
                {
                    var lockToBounds = LockUsableAreaDisplay || LockUsableAreaTablet;
                    profile.OutputMode[nameof(AbsoluteOutputMode.LockToBounds)].SetValue(lockToBounds);
                }
            }

            return new Desktop.Settings
            {
                Profiles = profiles,
                Tools = new PluginSettingsCollection(tools ?? Array.Empty<PluginSettings>()),
                Revision = Version.Parse("0.7.0.0")
            };
        }
    }
}
