using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenTabletDriver.Components;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    [JsonObject]
    internal class Profile : IMigrate<Profiles.Profile>
    {
        [JsonProperty("Tablet")]
        public string? Tablet { set; get; }

        [JsonProperty("OutputMode")]
        public PluginSettingStore? OutputMode { set; get; }

        [JsonProperty("Filters")]
        public Collection<PluginSettingStore>? Filters { set; get; }

        [JsonProperty("AbsoluteModeSettings")]
        public AbsoluteModeSettings? AbsoluteModeSettings { set; get; }

        [JsonProperty("RelativeModeSettings")]
        public RelativeModeSettings? RelativeModeSettings { set; get; }

        [JsonProperty("Bindings")]
        public BindingSettings? BindingSettings { set; get; }

        public Profiles.Profile Migrate(IServiceProvider serviceProvider)
        {
            var pluginFactory = serviceProvider.GetRequiredService<IPluginFactory>();
            var filters = Filters?.MigrateAll<PluginSettingStore, PluginSettings>(serviceProvider) ?? Array.Empty<PluginSettings>();

            var outputMode = OutputMode?.Migrate(serviceProvider) ?? new PluginSettings(typeof(AbsoluteMode));
            var type = pluginFactory.GetPluginType(outputMode.Path);

            var absoluteType = typeof(AbsoluteOutputMode);
            if (type?.IsAssignableTo(absoluteType) ?? false)
            {
                var input = AbsoluteModeSettings?.Tablet?.Migrate(serviceProvider) ?? GetDefaultTabletArea(serviceProvider);
                var output = AbsoluteModeSettings?.Display?.Migrate(serviceProvider) ?? GetDefaultDisplayArea(serviceProvider);
                var areaClipping = AbsoluteModeSettings?.EnableClipping;
                var areaLimiting = AbsoluteModeSettings?.EnableAreaLimiting;
                var lockAspectRatio = AbsoluteModeSettings?.LockAspectRatio;

                outputMode[nameof(AbsoluteOutputMode.Input)].SetValue(input);
                outputMode[nameof(AbsoluteOutputMode.Output)].SetValue(output);
                outputMode[nameof(AbsoluteOutputMode.AreaClipping)].Value = areaClipping ?? true;
                outputMode[nameof(AbsoluteOutputMode.AreaLimiting)].Value = areaLimiting ?? false;
                outputMode[nameof(AbsoluteOutputMode.LockAspectRatio)].Value = lockAspectRatio ?? false;
            }

            var relativeType = typeof(RelativeOutputMode);
            if (type?.IsAssignableTo(relativeType) ?? false)
            {
                var defaults = serviceProvider.GetDefaultSettings(relativeType);

                if (RelativeModeSettings != null)
                {
                    var sensitivity = new Vector2(RelativeModeSettings!.XSensitivity, RelativeModeSettings!.YSensitivity);
                    outputMode[nameof(RelativeOutputMode.Sensitivity)].SetValue(sensitivity);
                }
                else
                {
                    outputMode[nameof(RelativeOutputMode.Sensitivity)].Value = defaults[nameof(RelativeOutputMode.Sensitivity)].Value;
                }

                var rotation = RelativeModeSettings?.RelativeRotation;
                var resetDelay = RelativeModeSettings?.ResetTime;

                outputMode[nameof(RelativeOutputMode.Rotation)].Value = rotation ?? defaults[nameof(RelativeOutputMode.Rotation)].Value;
                outputMode[nameof(RelativeOutputMode.ResetDelay)].Value = resetDelay ?? defaults[nameof(RelativeOutputMode.ResetDelay)].Value;
            }

            return new Profiles.Profile
            {
                Tablet = Tablet!,
                OutputMode = outputMode,
                Filters = new PluginSettingsCollection(filters),
                Bindings = BindingSettings?.Migrate(serviceProvider) ?? new Profiles.BindingSettings(),
            };
        }

        private AngledArea GetDefaultTabletArea(IServiceProvider serviceProvider)
        {
            var configProvider = serviceProvider.GetRequiredService<IDeviceConfigurationProvider>();
            var config = configProvider.TabletConfigurations.First(c => c.Name == Tablet);
            var specs = config.Specifications.Digitizer!;
            return new AngledArea
            {
                Width = specs.Width,
                Height = specs.Height,
                XPosition = specs.Width / 2,
                YPosition = specs.Height / 2,
                Rotation = 0
            };
        }

        private Area GetDefaultDisplayArea(IServiceProvider serviceProvider)
        {
            var screen = serviceProvider.GetRequiredService<IVirtualScreen>();
            return new Area
            {
                Width = screen.Width,
                Height = screen.Height,
                XPosition = screen.Width / 2,
                YPosition = screen.Height / 2
            };
        }
    }
}
