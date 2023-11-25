using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTabletDriver.Components;
using OpenTabletDriver.Configurations;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon
{
    public class DesktopDeviceConfigurationProvider : IDeviceConfigurationProvider
    {
        private const int THRESHOLD_MS = 250;
        private readonly DeviceConfigurationProvider _inAssemblyConfigurationProvider = new();
        private readonly AppInfo _appInfo;
        private readonly FileSystemWatcher? _watcher;

        private CancellationTokenSource? _cts;
        private ImmutableArray<TabletConfiguration> _tabletConfigurations;

        public DesktopDeviceConfigurationProvider(AppInfo appInfo)
        {
            _appInfo = appInfo;

            _tabletConfigurations = GetTabletConfigurations();

            if (!Directory.Exists(_appInfo.ConfigurationDirectory))
                return;

            Log.Write("Detect", $"Using configuration overrides: '{_appInfo.ConfigurationDirectory}'", LogLevel.Debug);
            _watcher = new FileSystemWatcher(_appInfo.ConfigurationDirectory)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Filter = "*.json",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _watcher.Changed += debouncedUpdateConfigurations;
            _watcher.Renamed += debouncedUpdateConfigurations;
            _watcher.Created += debouncedUpdateConfigurations;
            _watcher.Deleted += debouncedUpdateConfigurations;

            // wait THRESHOLD_MS before updating configurations
            // if another change occurs within THRESHOLD_MS, cancel this update
            void debouncedUpdateConfigurations(object sender, FileSystemEventArgs e)
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var ct = _cts.Token;

                Task.Run(async () =>
                {
                    await Task.Delay(THRESHOLD_MS, ct);
                    Log.Debug("Detect", "Refreshing configurations...");
                    _tabletConfigurations = GetTabletConfigurations();
                    TabletConfigurationsChanged?.Invoke(_tabletConfigurations);
                }, ct);
            }
        }

        public bool RaisesTabletConfigurationsChanged => true;
        public ImmutableArray<TabletConfiguration> TabletConfigurations => _tabletConfigurations;
        public event Action<ImmutableArray<TabletConfiguration>>? TabletConfigurationsChanged;

        private ImmutableArray<TabletConfiguration> GetTabletConfigurations()
        {
            if (Directory.Exists(_appInfo.ConfigurationDirectory))
            {
                var jsonSerializer = new JsonSerializer();
                TabletConfiguration[] configOverrides = Directory.EnumerateFiles(_appInfo.ConfigurationDirectory, "*.json", SearchOption.AllDirectories)
                    .Select(f => ParseConfiguration(jsonSerializer, f))
                    .Where(config => config != null)
                    .ToArray()!;

                var configMap = new Dictionary<string, TabletConfiguration>();

                // populate configMap with all configurations from the assembly
                // and then populate it with overrides from the user's config directory,
                // replacing any existing configurations with the same name
                PopulateMap(configMap, _inAssemblyConfigurationProvider.TabletConfigurations);
                PopulateMap(configMap, configOverrides);

                return configMap.Values.OrderBy(config => config.Name).ToImmutableArray();
            }

            return _inAssemblyConfigurationProvider.TabletConfigurations;
        }

        private static TabletConfiguration? ParseConfiguration(JsonSerializer serializer, string path)
        {
            try
            {
                using (var sr = new StreamReader(path))
                using (var jr = new JsonTextReader(sr))
                    return serializer.Deserialize<TabletConfiguration>(jr);
            }
            catch (Exception e)
            {
                Log.Write("Detect", $"Failed to parse configuration at '{path}': {e.Message}", LogLevel.Error);
                return null;
            }
        }

        private static void PopulateMap(Dictionary<string, TabletConfiguration> configMap, IEnumerable<TabletConfiguration> configs)
        {
            foreach (var config in configs)
            {
                ref var configInMap = ref CollectionsMarshal.GetValueRefOrAddDefault(configMap, config.Name, out _);
                configInMap = config;
            }
        }
    }
}
