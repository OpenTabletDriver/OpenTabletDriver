using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTabletDriver.Configurations;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

#nullable enable

namespace OpenTabletDriver.Desktop
{
    public class DesktopDeviceConfigurationProvider : IDeviceConfigurationProvider
    {
        private readonly DeviceConfigurationProvider _inAssemblyConfigurationProvider = new();

        public IEnumerable<TabletConfiguration> TabletConfigurations => GetTabletConfigurations();

        private IEnumerable<TabletConfiguration> GetTabletConfigurations()
        {
            IEnumerable<(ConfigurationSource, TabletConfiguration)> jsonConfigurations = Array.Empty<(ConfigurationSource, TabletConfiguration)>();

            if (Directory.Exists(AppInfo.Current.ConfigurationDirectory))
            {
                Log.Write("Detect", $"Configuration overrides exist in '{AppInfo.Current.ConfigurationDirectory}', overriding built-in configurations.", LogLevel.Debug);
                var files = Directory.EnumerateFiles(AppInfo.Current.ConfigurationDirectory, "*.json", SearchOption.AllDirectories);

                jsonConfigurations = files.Select(path => Serialization.Deserialize<TabletConfiguration>(File.OpenRead(path)))
                    .Select(jsonConfig => (ConfigurationSource.File, jsonConfig));
            }

            return _inAssemblyConfigurationProvider.TabletConfigurations
                .Select(asmConfig => (ConfigurationSource.Assembly, asmConfig))
                .Concat(jsonConfigurations)
                .GroupBy(sourcedConfig => sourcedConfig.Item2.Name)
                .Select(multiSourcedConfig =>
                {
                    var asmConfig = multiSourcedConfig.Where(m => m.Item1 == ConfigurationSource.Assembly)
                        .Select(m => m.Item2)
                        .FirstOrDefault();
                    var jsonConfig = multiSourcedConfig.Where(m => m.Item1 == ConfigurationSource.File)
                        .Select(m => m.Item2)
                        .FirstOrDefault();

                    return jsonConfig ?? asmConfig!;
                });
        }

        private enum ConfigurationSource
        {
            Assembly,
            File
        }
    }
}
