using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Components;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Binding;
#if RELEASE
using OpenTabletDriver.Desktop.Components;
#endif
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.SystemDrivers;
using OpenTabletDriver.Tablet;

#nullable enable

namespace OpenTabletDriver.Daemon
{
    public class DriverDaemon : IDriverDaemon
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDriver _driver;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly ICompositeDeviceHub _deviceHub;
        private readonly IAppInfo _appInfo;
        private readonly ISettingsManager _settingsManager;
        private readonly IPluginManager _pluginManager;
        private readonly IPluginFactory _pluginFactory;
        private readonly IPresetManager _presetManager;
        private readonly IUpdater? _updater;

        public DriverDaemon(
            IServiceProvider serviceProvider,
            IDriver driver,
            SynchronizationContext synchronizationContext,
            ICompositeDeviceHub deviceHub,
            IAppInfo appInfo,
            ISettingsManager settingsManager,
            IPluginManager pluginManager,
            IPluginFactory pluginFactory,
            IPresetManager presetManager
        )
        {
            _serviceProvider = serviceProvider;
            _driver = driver;
            _synchronizationContext = synchronizationContext;
            _deviceHub = deviceHub;
            _appInfo = appInfo;
            _settingsManager = settingsManager;
            _pluginManager = pluginManager;
            _pluginFactory = pluginFactory;
            _presetManager = presetManager;

            _updater = serviceProvider.GetService<IUpdater>();
        }

        public async Task Initialize()
        {
            Log.Output += (sender, message) =>
            {
                LogMessages.Add(message);
                Console.WriteLine(Log.GetStringFormat(message));
                Message?.Invoke(sender, message);
            };

            _driver.InputDevicesChanged += (sender, e) => TabletsChanged?.Invoke(sender, e.Select(c => c.Configuration));
            _deviceHub.DevicesChanged += (_, e) =>
            {
                if (!e.Additions.Any())
                    return;

                // ReSharper disable once AsyncVoidLambda
                _synchronizationContext.Post(async _ =>
                {
                    await DetectTablets();
                    await ApplySettings(_settingsManager.Settings);
                }, this);
            };

            _pluginManager.AssembliesChanged += (s, e) => AssembliesChanged?.Invoke(s, e);

            foreach (var driverInfo in DriverInfo.GetDriverInfos())
            {
                Log.Write("Detect", $"Another tablet driver found: {driverInfo.Name}", LogLevel.Warning);
                if (driverInfo.IsBlockingDriver)
                    Log.Write("Detect", $"Detection for {driverInfo.Name} tablets might be impaired", LogLevel.Warning);
                else if (driverInfo.IsSendingInput)
                    Log.Write("Detect", $"Detected input coming from {driverInfo.Name} driver", LogLevel.Error);
            }

            await LoadUserSettings();

#if RELEASE
            SleepDetection = new SleepDetectionThread(() =>
            {
                Log.Write(nameof(SleepDetectionThread), "Sleep detected...", LogLevel.Debug);
                _ = DetectTablets();
            });

            SleepDetection.Start();
#endif
        }

        public event EventHandler<LogMessage>? Message;
        public event EventHandler<DebugReportData>? DeviceReport;
        public event EventHandler<IEnumerable<TabletConfiguration>>? TabletsChanged;
        public event EventHandler<Settings>? SettingsChanged;
        public event EventHandler<PluginEventType>? AssembliesChanged;

        private Collection<LogMessage> LogMessages { get; } = new Collection<LogMessage>();
        private Collection<ITool> Tools { get; } = new Collection<ITool>();
#if RELEASE
        private SleepDetectionThread? SleepDetection { set; get; }
#endif

        private bool _debugging;

        public Task WriteMessage(LogMessage message)
        {
            Log.Write(message);
            return Task.CompletedTask;
        }

        public Task<bool> CheckAssemblyHashes(string remoteHash)
        {
            var localHash = _pluginManager.GetStateHash();
            return Task.FromResult(localHash == remoteHash);
        }

        public Task<bool> InstallPlugin(string filePath)
        {
            return Task.FromResult(_pluginManager.InstallPlugin(filePath));
        }

        public Task<bool> UninstallPlugin(PluginMetadata metadata)
        {
            var context = _pluginManager.Plugins.First(ctx => ctx.GetMetadata().Match(metadata));
            return Task.FromResult(_pluginManager.UninstallPlugin(context));
        }

        public Task<bool> DownloadPlugin(PluginMetadata metadata)
        {
            return _pluginManager.DownloadPlugin(metadata);
        }

        public async Task<IEnumerable<PluginMetadata>> GetRemotePlugins(string owner, string name, string gitRef)
        {
            return await PluginMetadataCollection.DownloadAsync(_appInfo, owner, name, gitRef);
        }

        public Task<IEnumerable<PluginMetadata>> GetInstalledPlugins()
        {
            return Task.FromResult(_pluginManager.Plugins.Select(p => p.GetMetadata()));
        }

        public Task<IEnumerable<TabletConfiguration>> GetTablets()
        {
            return Task.FromResult(_driver.InputDevices.Select(c => c.Configuration));
        }

        public async Task<IEnumerable<TabletConfiguration>> DetectTablets()
        {
            _driver.Detect();

            foreach (var tablet in _driver.InputDevices)
            {
                foreach (var dev in tablet.Endpoints)
                {
                    dev.RawReport += (_, report) => PostDebugReport(dev.Configuration.ToString(), report);
                    dev.RawClone = _debugging;
                }
            }

            await ApplySettings(_settingsManager.Settings);
            return await GetTablets();
        }

        public async Task SaveSettings(Settings settings)
        {
            settings.Serialize(new FileInfo(_appInfo.SettingsFile));
            Log.Write("Settings", $"Settings saved to '{_appInfo.SettingsFile}'");
            await ApplySettings(settings);
        }

        public Task ApplySettings(Settings? settings)
        {
            // Dispose filters that implement IDisposable interface
            foreach (var obj in _driver.InputDevices.SelectMany(d =>
                         d.OutputMode?.Elements ?? (IEnumerable<object>) Array.Empty<object>()))
                if (obj is IDisposable disposable)
                    disposable.Dispose();

            _settingsManager.Settings = settings ?? Settings.GetDefaults();

            foreach (var device in _driver.InputDevices.ToImmutableArray())
            {
                var group = device.Configuration.ToString();

                var profile = _settingsManager.Settings.Profiles.GetOrSetDefaults(_serviceProvider, device);
                device.OutputMode = _pluginFactory.Construct<IOutputMode>(profile.OutputMode, device);

                if (device.OutputMode != null)
                {
                    var outputModeName = _pluginFactory.GetName(profile.OutputMode);
                    Log.Write(group, $"Output mode: {outputModeName}");
                }

                if (device.OutputMode is IOutputMode outputMode)
                {
                    SetOutputModeSettings(device, outputMode, profile);

                    var mouseButtonHandler = (outputMode as IMouseButtonSource)?.MouseButtonHandler;

                    var deps = new object?[]
                    {
                        device,
                        profile.BindingSettings,
                        mouseButtonHandler
                    }.Where(o => o != null).ToArray() as object[];

                    var bindingHandler = _serviceProvider.CreateInstance<BindingHandler>(deps);

                    var lastElement = outputMode.Elements?.LastOrDefault() ??
                                      outputMode as IPipelineElement<IDeviceReport>;
                    lastElement.Emit += bindingHandler.Consume;
                }
            }

            Log.Write("Settings", "Driver is enabled.");

            SetToolSettings();

            SettingsChanged?.Invoke(this, _settingsManager.Settings);

            return Task.CompletedTask;
        }

        public async Task<Settings> ResetSettings()
        {
            await ApplySettings(Settings.GetDefaults());
            return await GetSettings();
        }

        private async Task LoadUserSettings()
        {
            var appdataDir = new DirectoryInfo(_appInfo.AppDataDirectory);
            if (!appdataDir.Exists)
            {
                appdataDir.Create();
                Log.Write("Settings", $"Created OpenTabletDriver application data directory: {appdataDir.FullName}");
            }

            _pluginManager.Clean();
            _pluginManager.Load();

            _settingsManager.Load();

            await DetectTablets();
        }

        private void SetOutputModeSettings(InputDevice dev, IOutputMode outputMode, Profile profile)
        {
            var group = dev.Configuration.ToString();

            var elements = from store in profile.Filters
                where store.Enable
                let filter = _pluginFactory.Construct<IDevicePipelineElement>(store, dev)
                where filter != null
                select filter;

            outputMode.Elements = elements.ToList();

            if (outputMode.Elements.Any())
                Log.Write(group, $"Filters: {string.Join(", ", outputMode.Elements)}");
        }

        private void SetToolSettings()
        {
            foreach (var runningTool in Tools)
                runningTool.Dispose();
            Tools.Clear();

            foreach (var settings in _settingsManager.Settings.Tools)
            {
                if (settings is { Enable: false })
                    continue;

                var tool = _pluginFactory.Construct<ITool>(settings!);

                if (tool?.Initialize() ?? false)
                {
                    Tools.Add(tool);
                }
                else
                {
                    var name = _pluginFactory.GetName(settings);
                    Log.Write("Tool", $"Failed to initialize {name} tool.", LogLevel.Error);
                }
            }
        }

        public Task<Settings> GetSettings()
        {
            return Task.FromResult(_settingsManager.Settings);
        }

        public async Task ApplyPreset(string name)
        {
            if (_presetManager.LoadPreset(name) is Preset preset)
                await ApplySettings(preset.Settings);
            else
                Log.Write("Presets", $"Unable apply preset \"{name}\" as it does not exist.", LogLevel.Error);
        }

        public Task<IReadOnlyCollection<string>> GetPresets()
        {
            return Task.FromResult(_presetManager.GetPresets());
        }

        public Task SavePreset(string name, Settings settings)
        {
            _presetManager.Save(name, settings);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<IDeviceEndpoint>> GetDevices()
        {
            return Task.FromResult(_deviceHub.GetDevices());
        }

        public Task<IEnumerable<IDisplay>> GetDisplays()
        {
            return Task.FromResult(_serviceProvider.GetRequiredService<IVirtualScreen>().Displays
                .Where(t => t is not IVirtualScreen));
        }

        public Task<IAppInfo> GetApplicationInfo()
        {
            return Task.FromResult(_appInfo);
        }

        public async Task<IDiagnosticInfo> GetDiagnostics()
        {
            var devices = await GetDevices();
            return ActivatorUtilities.CreateInstance<DiagnosticInfo>(_serviceProvider, LogMessages, devices);
        }

        public Task SetTabletDebug(bool enabled)
        {
            _debugging = enabled;
            foreach (var endpoint in _driver.InputDevices.SelectMany(d => d.Endpoints))
            {
                endpoint.RawClone = _debugging;
            }

            Log.Debug("Tablet", $"Tablet debugging is {(_debugging ? "enabled" : "disabled")}");

            return Task.CompletedTask;
        }

        public Task<string?> RequestDeviceString(int vid, int pid, int index)
        {
            var tablet = _deviceHub.GetDevices().FirstOrDefault(d => d.VendorID == vid && d.ProductID == pid);
            if (tablet == null)
                throw new IOException($"Device not found ({vid:X2}:{pid:X2})");

            return Task.FromResult(tablet.GetDeviceString((byte) index));
        }

        public Task<IEnumerable<LogMessage>> GetCurrentLog()
        {
            return Task.FromResult((IEnumerable<LogMessage>) LogMessages);
        }

        public Task<PluginSettings> GetDefaults(string path)
        {
            var type = _pluginFactory.GetPluginType(path)!;
            var settings = _serviceProvider.GetDefaultSettings(type, this);
            return Task.FromResult(settings);
        }

        public Task<TypeProxy> GetProxiedType(string typeName)
        {
            var type = _pluginManager.ExportedTypes.First(t => t.GetPath() == typeName);
            var proxy = ActivatorUtilities.CreateInstance<TypeProxy>(_serviceProvider, type);
            return Task.FromResult(proxy);
        }

        public Task<IEnumerable<TypeProxy>> GetMatchingTypes(string typeName)
        {
            var baseType = _pluginManager.ExportedTypes.First(t => t.GetPath() == typeName);
            var matchingTypes = from type in _pluginFactory.GetMatchingTypes(baseType)
                select ActivatorUtilities.CreateInstance<TypeProxy>(_serviceProvider, type);
            return Task.FromResult(matchingTypes);
        }

        public Task<bool> HasUpdate()
        {
            return _updater?.CheckForUpdates() ?? Task.FromResult(false);
        }

        public async Task<UpdateInfo?> GetUpdateInfo()
        {
            if (_updater == null)
                return null;

            return await _updater.GetInfo();
        }

        public Task InstallUpdate()
        {
            return _updater?.InstallUpdate() ?? Task.CompletedTask;
        }

        private void PostDebugReport(string deviceName, IDeviceReport report)
        {
            DeviceReport?.Invoke(this, new DebugReportData(deviceName, report));
        }
    }
}
