using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
        private readonly ISettingsManager _settingsManager;
        private readonly IPluginManager _pluginManager;
        private readonly IPluginFactory _pluginFactory;
        private readonly IPresetManager _presetManager;
        private readonly ISleepDetector _sleepDetector;
        private readonly IUpdater? _updater;
        private readonly LogFile _logFile;

        private UpdateInfo? _updateInfo;
        private Settings? _lastValidSettings;

        public DriverDaemon(
            IServiceProvider serviceProvider,
            IDriver driver,
            SynchronizationContext synchronizationContext,
            ICompositeDeviceHub deviceHub,
            IAppInfo appInfo,
            ISettingsManager settingsManager,
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
            _logFile = new LogFile(_appInfo.LogDirectory);

            _updater = serviceProvider.GetService<IUpdater>();
        }

        public async Task Initialize()
        {
            Log.Output += (sender, message) =>
            {
                _logFile.Write(message);
                Console.WriteLine(Log.GetStringFormat(message));
                Message?.Invoke(sender, message);
            };

            InitializePlatform();
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
                var os = SystemInterop.CurrentPlatform switch
                {
                    SystemPlatform.Windows => "Windows",
                    SystemPlatform.Linux => "Linux",
                    SystemPlatform.MacOS => "MacOS",
                    _ => null
                };
                var wikiUrl = $"https://opentabletdriver.net/Wiki/FAQ/{os}";

                var message = new StringBuilder();
                message.Append($"'{driverInfo.Name}' driver is detected.");

                if (driverInfo.Status.HasFlag(DriverStatus.Blocking))
                    message.Append(" It will block detection of tablets.");
                if (driverInfo.Status.HasFlag(DriverStatus.Flaky))
                    message.Append(" It will cause flaky support to tablets.");
                if (os != null)
                    message.Append($" If any problems arise, visit '{wikiUrl}'.");

                Log.WriteNotify("Detect", message.ToString(), LogLevel.Warning);
            }

            await LoadUserSettings();

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
        public event EventHandler<IEnumerable<TabletConfiguration>>? TabletsChanged;
        public event EventHandler<Settings>? SettingsChanged;
        public event EventHandler<PluginEventType>? AssembliesChanged;
        public event EventHandler? Resynchronize;

        private Collection<ITool> Tools { get; } = new Collection<ITool>();

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
                hookEndpoint(tablet.Digitizer);
                hookEndpoint(tablet.Auxiliary);
            }

            await ApplySettings(_settingsManager.Settings);
            return await GetTablets();

            void hookEndpoint(InputDeviceEndpoint? endpoint)
            {
                if (endpoint is null)
                    return;

                endpoint.ReportCloned += (_, report) => PostDebugReport(endpoint.Configuration.Name, report);
                endpoint.CloneReport = _debugging;
            }
        }

        public async Task SaveSettings(Settings settings)
        {
            settings.Serialize(new FileInfo(_appInfo.SettingsFile));
            Log.Write("Settings", $"Settings saved to '{_appInfo.SettingsFile}'");
            await ApplySettings(settings);
        }

        public Task ApplySettings(Settings? settings)
        {
            try
            {
                // Dispose filters that implement IDisposable interface
                foreach (var obj in _driver.InputDevices.SelectMany(d =>
                            d.OutputMode?.Elements ?? (IEnumerable<object>)Array.Empty<object>()))
                    if (obj is IDisposable disposable)
                        disposable.Dispose();

                _settingsManager.Settings = settings ?? Settings.GetDefaults();

                foreach (var device in _driver.InputDevices)
                {
                    var group = device.Configuration.Name;

                    var profile = _settingsManager.Settings.Profiles.GetOrSetDefaults(_serviceProvider, device);
                    device.OutputMode = _pluginFactory.Construct<IOutputMode>(profile.OutputMode, device);

                    if (device.OutputMode != null)
                    {
                        var outputModeName = _pluginFactory.GetName(profile.OutputMode);
                        Log.Write(group, $"Output mode: {outputModeName}");

                        var outputMode = device.OutputMode;

                        var mouseButtonHandler = (outputMode as IMouseButtonSource)?.MouseButtonHandler;

                        var deps = new object?[]
                        {
                            device,
                            profile.BindingSettings,
                            mouseButtonHandler
                        }.Where(o => o != null).ToArray() as object[];

                        var bindingHandler = _serviceProvider.CreateInstance<BindingHandler>(deps);
                        SetOutputModeElements(device, outputMode, profile, bindingHandler);
                    }
                }

                if (_driver.InputDevices.Length != 0)
                    Log.Write("Settings", "Driver is enabled.");

                SetToolSettings();

                _lastValidSettings = settings;
                SettingsChanged?.Invoke(this, _settingsManager.Settings);

                return Task.CompletedTask;
            }
            catch
            {
                try
                {
                    ApplySettings(_lastValidSettings);
                    Log.WriteNotify("Settings", "Failed to apply settings. Reverted to last valid settings.", LogLevel.Warning);
                }
                catch
                {
                    ApplySettings(null);
                    Log.WriteNotify("Settings", "Failed to apply last valid settings. Forcibly applied defaults.", LogLevel.Warning);
                }

                Resynchronize?.Invoke(this, EventArgs.Empty);
                return Task.CompletedTask;
            }
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

        private void SetOutputModeElements(InputDevice dev, IOutputMode outputMode, Profile profile, BindingHandler bindingHandler)
        {
            string group = dev.Configuration.Name;

            var elements = from store in profile.Filters
                           where store.Enable
                           let filter = _pluginFactory.Construct<IDevicePipelineElement>(store, dev)
                           where filter != null
                           select filter;

            outputMode.Elements = elements.Append(bindingHandler).ToList();

            if (outputMode.Elements.Count > 1)
                Log.Write(group, $"Filters: {string.Join(", ", outputMode.Elements.Where(e => e != bindingHandler))}");
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
            return ActivatorUtilities.CreateInstance<DiagnosticInfo>(_serviceProvider, _logFile.Read(), devices);
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
            return Task.FromResult(_logFile.Read());
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

        private void PostDebugReport(string tablet, IDeviceReport report)
        {
            DeviceReport?.Invoke(this, new DebugReportData(tablet, report));
        }

        private static void InitializePlatform()
        {
            switch (SystemInterop.CurrentPlatform)
            {
                case SystemPlatform.Windows:
                    System.Diagnostics.Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

                    if (Environment.OSVersion.Version.Build >= 22000) // Windows 11
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
                                Log.Write("Platform", "Failed to allow management of timer resolution, asynchronous filters may have lower timing resolution when OTD is minimized.", LogLevel.Error);
                            }
                        }
                    }
                    break;
            }
        }
    }
}
