using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    [JsonObject]
    internal class BindingSettings : IMigrate<Profiles.BindingSettings>
    {
        [JsonProperty("TipActivationThreshold")]
        public float TipActivationThreshold { set; get; }

        [JsonProperty("TipButton")]
        public PluginSettingStore? TipButton { set; get; }

        [JsonProperty("EraserActivationThreshold")]
        public float EraserActivationThreshold { set; get; }

        [JsonProperty("EraserButton")]
        public PluginSettingStore? EraserButton { set; get; }

        [JsonProperty("PenButtons")]
        public Collection<PluginSettingStore>? PenButtons { set; get; }

        [JsonProperty("AuxButtons")]
        public Collection<PluginSettingStore>? AuxButtons { set; get; }

        [JsonProperty("MouseButtons")]
        public Collection<PluginSettingStore>? MouseButtons { set; get; }

        [JsonProperty("MouseScrollUp")]
        public PluginSettingStore? MouseScrollUp { set; get; }

        [JsonProperty("MouseScrollDown")]
        public PluginSettingStore? MouseScrollDown { set; get; }

        public Profiles.BindingSettings Migrate(IServiceProvider serviceProvider)
        {
            var auxButtons = AuxButtons?.MigrateAll<PluginSettingStore, PluginSettings>(serviceProvider);
            var penButtons = PenButtons?.MigrateAll<PluginSettingStore, PluginSettings>(serviceProvider);
            var mouseButtons = MouseButtons?.MigrateAll<PluginSettingStore, PluginSettings>(serviceProvider);

            return new Profiles.BindingSettings
            {
                TipButton = TipButton?.Migrate(serviceProvider),
                TipActivationThreshold = TipActivationThreshold,
                EraserButton = EraserButton?.Migrate(serviceProvider),
                EraserActivationThreshold = EraserActivationThreshold,
                PenButtons = new PluginSettingsCollection(penButtons ?? Array.Empty<PluginSettings>()),
                AuxButtons = new PluginSettingsCollection(auxButtons ?? Array.Empty<PluginSettings>()),
                MouseButtons = new PluginSettingsCollection(mouseButtons ?? Array.Empty<PluginSettings>()),
                MouseScrollDown = MouseScrollDown?.Migrate(serviceProvider),
                MouseScrollUp = MouseScrollUp?.Migrate(serviceProvider)
            };
        }
    }
}
