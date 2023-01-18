using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Components;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Components;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Logging;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.SystemDrivers;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon
{
    public class DriverDaemon : IDriverDaemon
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDriver _driver;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly ICompositeDeviceHub _deviceHub;
        private readonly IAppInfo _appInfo;
        private readonly ISettingsPersistenceManager _settingsManager;
        private readonly IPluginManager _pluginManager;
        private readonly IPluginFactory _pluginFactory;
        private readonly IPresetManager _presetManager;
        private readonly ISleepDetector _sleepDetector;
        private readonly IUpdater? _updater;

        private readonly Collection<LogMessage> _logMessages = new();
        private readonly Collection<ITool> _tools = new();

        private Settings? _settings;
        private UpdateInfo? _updateInfo;
        private bool _debugging;
        private bool _isScanningDevices;

        public DriverDaemon(
            IServiceProvider serviceProvider,
            IDriver driver,
            SynchronizationContext synchronizationContext,
            ICompositeDeviceHub deviceHub,
            IAppInfo appInfo,
            ISettingsPersistenceManager settingsManager,
            IPluginManager pluginManager,
            IPluginFactory pluginFactory,
            IPresetManager presetManager,
            ISleepDetector sleepDetector
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
            _sleepDetector = sleepDetector;

            _updater = serviceProvider.GetService<IUpdater>();
        }

        public async Task Initialize()
        {
            Log.Output += (sender, message) =>
            {
                _logMessages.Add(message);
                Console.WriteLine(Log.GetStringFormat(message));
                Message?.Invoke(sender, message);
            };

            Log.Write("Detect", $"Configuration overrides exist: '{_appInfo.ConfigurationDirectory}'", LogLevel.Debug);
            InitializePlatform();
            _driver.InputDeviceAdded += (sender, e) =>
            {
                hookEndpoint(e.Digitizer);
                hookEndpoint(e.Auxiliary);

                var profile = _settings!.GetProfile(_serviceProvider, e);

                // avoid applying settings twice when "redetecting"
                if (!_isScanningDevices)
                    SetTabletSettings(e, profile);

                TabletAdded?.Invoke(sender, e.Id);
                e.StateChanged += (s, state) => TabletStateChanged?.Invoke(s, new TabletProperty<InputDeviceState>(e.Id, state));

                void hookEndpoint(InputDeviceEndpoint? endpoint)
                {
                    if (endpoint is null)
                        return;

                    endpoint.ReportCloned += (_, report) => PostDebugReport(endpoint.Configuration.Name, report);
                    endpoint.CloneReport = _debugging;
                }
            };

            _driver.InputDeviceRemoved += (sender, e) =>
            {
                TabletRemoved?.Invoke(sender, e.Id);
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

            var appdataDir = new DirectoryInfo(_appInfo.AppDataDirectory);
            if (!appdataDir.Exists)
            {
                appdataDir.Create();
                Log.Write("Settings", $"Created OpenTabletDriver application data directory: {appdataDir.FullName}");
            }

            _pluginManager.Clean();
            _pluginManager.Load();

            _settings = _settingsManager.Load(new FileInfo(_appInfo.SettingsFile)) ?? new Settings();

            await DetectTablets();

            _sleepDetector.Slept += async () =>
            {
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                    return;
#endif
                Log.Write(nameof(DriverDaemon), "Sleep detected...", LogLevel.Info);
                await DetectTablets();
            };
        }

        public event EventHandler<LogMessage>? Message;
        public event EventHandler<DebugReportData>? DeviceReport;
        public event EventHandler<int>? TabletAdded;
        public event EventHandler<int>? TabletRemoved;
        public event EventHandler<TabletProperty<InputDeviceState>>? TabletStateChanged;
        public event EventHandler<TabletProperty<Profile>>? TabletProfileChanged;
        public event EventHandler<Settings>? SettingsChanged;
        public event EventHandler<PluginEventType>? AssembliesChanged;

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

        public Task<IEnumerable<IDeviceEndpoint>> GetDevices()
        {
            return Task.FromResult(_deviceHub.GetDevices());
        }

        public Task<IEnumerable<IDisplay>> GetDisplays()
        {
            return Task.FromResult(_serviceProvider.GetRequiredService<IVirtualScreen>().Displays
                .Where(t => t is not IVirtualScreen));
        }

        public async Task DetectTablets()
        {
            _isScanningDevices = true;
            _driver.ScanDevices();
            await ApplySettings(_settings);
            _isScanningDevices = false;
        }

        public Task<IEnumerable<int>> GetTablets()
        {
            return Task.FromResult(_driver.InputDevices.Select(c => c.Id));
        }

        public Task<int> GetPersistentId(int tabletId)
        {
            var tablet = FindTablet(tabletId);
            return Task.FromResult(tablet.PersistentId!.Value);
        }

        public Task<TabletConfiguration> GetTabletConfiguration(int tabletId)
        {
            var tablet = FindTablet(tabletId);
            return Task.FromResult(tablet.Configuration);
        }

        public Task<InputDeviceState> GetTabletState(int tabletId)
        {
            var tablet = FindTablet(tabletId);
            return Task.FromResult(tablet.State);
        }

        public Task<Profile> GetTabletProfile(int tabletId)
        {
            var profile = FindProfile(tabletId);
            return Task.FromResult(profile);
        }

        public Task SetTabletProfile(int tabletId, Profile profile)
        {
            var tablet = FindTablet(tabletId);
            SetTabletSettings(tablet, profile);
            TabletProfileChanged?.Invoke(this, new TabletProperty<Profile>(tabletId, profile));
            return Task.CompletedTask;
        }

        public Task ResetTabletProfile(int tabletId)
        {
            var tablet = FindTablet(tabletId);
            var profile = Profile.GetDefaults(_serviceProvider, tablet);
            _settings!.SetProfile(profile);
            SetTabletSettings(tablet, profile);
            TabletProfileChanged?.Invoke(this, new TabletProperty<Profile>(tabletId, profile));
            return Task.CompletedTask;
        }

        public async Task SaveSettings()
        {
            _settings!.Serialize(new FileInfo(_appInfo.SettingsFile));
            Log.Write("Settings", $"Settings saved to '{_appInfo.SettingsFile}'");
            await ApplySettings(_settings);
        }

        public Task ApplySettings(Settings? settings)
        {
            foreach (var device in _driver.InputDevices)
            {
                var profile = settings!.GetProfile(_serviceProvider, device);
                SetTabletSettings(device, profile);
            }

            SetToolSettings();
            SettingsChanged?.Invoke(this, _settings!);

            return Task.CompletedTask;
        }

        public async Task<Settings> ResetSettings()
        {
            await ApplySettings(new Settings());
            return await GetSettings();
        }

        public Task<Settings> GetSettings()
        {
            return Task.FromResult(_settings!);
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

        public Task<IAppInfo> GetApplicationInfo()
        {
            return Task.FromResult(_appInfo);
        }

        public async Task<IDiagnosticInfo> GetDiagnostics()
        {
            var devices = await GetDevices();
            return ActivatorUtilities.CreateInstance<DiagnosticInfo>(_serviceProvider, _logMessages, devices);
        }

        public Task SetTabletDebug(bool enabled)
        {
            _debugging = enabled;
            foreach (var inputDevice in _driver.InputDevices)
            {
                inputDevice.Digitizer.CloneReport = _debugging;

                if (inputDevice.Auxiliary is not null)
                    inputDevice.Auxiliary.CloneReport = _debugging;
            }

            Log.Debug("Tablet", $"Tablet debugging is {(_debugging ? "enabled" : "disabled")}");

            return Task.CompletedTask;
        }

        public Task<string?> RequestDeviceString(int vid, int pid, int index)
        {
            var tablet = _deviceHub.GetDevices().FirstOrDefault(d => d.VendorID == vid && d.ProductID == pid);
            if (tablet == null)
                throw new IOException($"Device not found ({vid:X2}:{pid:X2})");

            return Task.FromResult(tablet.GetDeviceString((byte)index));
        }

        public Task<IEnumerable<LogMessage>> GetCurrentLog()
        {
            return Task.FromResult((IEnumerable<LogMessage>)_logMessages);
        }

        public Task<PluginSettings> GetDefaults(string path)
        {
            var type = _pluginFactory.GetPluginType(path)!;
            var settings = _serviceProvider.GetDefaultSettings(type, this);
            return Task.FromResult(settings);
        }

        public Task<IEnumerable<TypeProxy>> GetMatchingTypes(string typeName)
        {
            var baseType = _pluginManager.ExportedTypes.First(t => t.GetPath() == typeName);
            var matchingTypes = from type in _pluginFactory.GetMatchingTypes(baseType)
                select ActivatorUtilities.CreateInstance<TypeProxy>(_serviceProvider, type);
            return Task.FromResult(matchingTypes);
        }

        public async Task<SerializedUpdateInfo?> CheckForUpdates()
        {
            if (_updater == null)
                return null;

            _updateInfo = await _updater.CheckForUpdates();
            return _updateInfo?.ToSerializedUpdateInfo();
        }

        public async Task InstallUpdate()
        {
            if (_updateInfo == null)
                throw new InvalidOperationException("No update available"); // Misbehaving client

            try
            {
                var update = await _updateInfo.GetUpdate();
                _updater?.Install(update);
            }
            catch
            {
                throw;
            }
            finally
            {
                _updateInfo = null;
            }
        }

        private InputDevice FindTablet(int tabletId)
        {
            var tablet = _driver.InputDevices.FirstOrDefault(c => c.Id == tabletId);
            if (tablet == null)
                throw new ArgumentException("Tablet not found", nameof(tabletId));

            return tablet;
        }

        private Profile FindProfile(int tabletId)
        {
            var tablet = FindTablet(tabletId);
            return _settings!.GetProfile(_serviceProvider, tablet);
        }

        private void SetTabletSettings(InputDevice inputDevice, Profile profile)
        {
            var persistentName = inputDevice.PersistentName;
            Log.Write("Settings", $"Applying settings for '{persistentName}'");
            foreach (var element in inputDevice.OutputMode?.Elements ?? Enumerable.Empty<IDevicePipelineElement>())
            {
                if (element is IDisposable disposable)
                    disposable.Dispose();
            }

            var outputMode = _pluginFactory.Construct<IOutputMode>(profile.OutputMode, inputDevice);

            if (outputMode is not null)
            {
                var outputModeName = _pluginFactory.GetName(profile.OutputMode);
                Log.Write("Settings", $"Output mode: {outputModeName}");

                SetOutputModeSettings(inputDevice, outputMode, profile);
                var mouseButtonHandler = (outputMode as IMouseButtonSource)?.MouseButtonHandler;

                var deps = new object?[]
                {
                    inputDevice,
                    outputMode,
                    profile.Bindings,
                    mouseButtonHandler
                }.Where(o => o != null).ToArray() as object[];

                var bindingHandler = _serviceProvider.CreateInstance<BindingHandler>(deps);

                var lastElement = outputMode.Elements?.LastOrDefault() ??
                                    outputMode as IPipelineElement<IDeviceReport>;
                lastElement.Emit += bindingHandler.Consume;
            }

            // set the output mode atomically
            inputDevice.OutputMode = outputMode;
            Log.Write("Settings", $"Settings applied for '{persistentName}'");
        }

        private void SetOutputModeSettings(InputDevice dev, IOutputMode outputMode, Profile profile)
        {
            string group = dev.Configuration.Name;

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
            foreach (var runningTool in _tools)
                runningTool.Dispose();
            _tools.Clear();

            foreach (var settings in _settings!.Tools)
            {
                if (settings is { Enable: false })
                    continue;

                var tool = _pluginFactory.Construct<ITool>(settings!);

                if (tool?.Initialize() ?? false)
                {
                    _tools.Add(tool);
                }
                else
                {
                    var name = _pluginFactory.GetName(settings);
                    Log.Write("Tool", $"Failed to initialize {name} tool.", LogLevel.Error);
                }
            }
        }

        private void PostDebugReport(string tablet, IDeviceReport report)
        {
            DeviceReport?.Invoke(this, new DebugReportData(tablet, report));
        }

        private static void InitializePlatform()
        {
            switch (SystemInterop.CurrentPlatform)
            {
                case SystemPlatform.Windows:
                    System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

                    var windows8 = new Version(6, 2, 9200, 0);
                    if (Environment.OSVersion.Version >= windows8)
                    {
                        unsafe
                        {
                            var state = Windows.PowerThrottlingState.Create();
                            state.ControlMask = (int)Windows.PowerThrottlingStateMask.IgnoreTimerResolution;

                            if (!Windows.SetProcessInformation(
                                System.Diagnostics.Process.GetCurrentProcess().Handle,
                                Windows.ProcessInformationClass.ProcessPowerThrottling,
                                (IntPtr)Unsafe.AsPointer(ref state),
                                Unsafe.SizeOf<Windows.PowerThrottlingState>()))
                            {
                                Log.Write("Platform", "Failed to allow timer resolution, asynchronous filters may have lower resolution when OTD is minimized.", LogLevel.Error);
                            }
                        }
                    }
                    break;
            }
        }
    }
}
