using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Components;
using OpenTabletDriver.Daemon.Binding;
using OpenTabletDriver.Daemon.Components;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.Daemon.Output;
using OpenTabletDriver.Daemon.Reflection;
using OpenTabletDriver.Daemon.Updater;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Logging;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;
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
        private readonly AppInfo _appInfo;
        private readonly ISettingsPersistenceManager _settingsManager;
        private readonly IPluginManager _pluginManager;
        private readonly IPluginFactory _pluginFactory;
        private readonly IPresetManager _presetManager;
        private readonly ISleepDetector _sleepDetector;
        private readonly IUpdater? _updater;

        private readonly Collection<LogMessage> _logMessages = new();
        private readonly Dictionary<int, (InputDevice, Profile)> _tablets = new();
        private readonly Collection<PluginSettings> _toolSettings = new();
        private readonly Collection<ITool> _tools = new();

        private ImmutableArray<PluginContextDto> _plugins = ImmutableArray<PluginContextDto>.Empty;
        private UpdateInfo? _updateInfo;
        private Settings? _lastValidSettings;
        private bool _debugging;
        private bool _isScanningDevices;

        public DriverDaemon(
            IServiceProvider serviceProvider,
            IDriver driver,
            SynchronizationContext synchronizationContext,
            ICompositeDeviceHub deviceHub,
            AppInfo appInfo,
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

        public event EventHandler<LogMessage>? Message;
        public event EventHandler<int>? TabletAdded;
        public event EventHandler<int>? TabletRemoved;
        public event EventHandler<TabletProperty<InputDeviceState>>? TabletStateChanged;
        public event EventHandler<TabletProperty<Profile>>? TabletProfileChanged;
        public event EventHandler<IEnumerable<PluginSettings>>? ToolsChanged;
        public event EventHandler<PluginContextDto>? PluginAdded;
        public event EventHandler<PluginContextDto>? PluginRemoved;

        public async Task Initialize()
        {
            Log.Output += (sender, message) =>
            {
                _logMessages.Add(message);
                Console.WriteLine(Log.GetStringFormat(message));
                Message?.Invoke(sender, message);
            };

            InitializePlatform();
            _driver.InputDeviceAdded += (sender, e) =>
            {
                // hookEndpoint(e.Digitizer);
                // hookEndpoint(e.Auxiliary);

                var profile = GetOrCreateProfile(_serviceProvider, e);

                // avoid applying settings twice when "redetecting"
                if (!_isScanningDevices)
                    ApplyTabletSettings(e, profile);

                TabletAdded?.Invoke(sender, e.Id);
                e.StateChanged += (s, state) => TabletStateChanged?.Invoke(s, new TabletProperty<InputDeviceState>(e.Id, state));

                // void hookEndpoint(InputDeviceEndpoint? endpoint)
                // {
                //     if (endpoint is null)
                //         return;

                //     endpoint.ReportCloned += (_, report) => PostDebugReport(endpoint.Configuration.Name, report);
                //     endpoint.CloneReport = _debugging;
                // }
            };

            _driver.InputDeviceRemoved += (sender, e) =>
            {
                _tablets.Remove(e.Id);
                TabletRemoved?.Invoke(sender, e.Id);
            };

            _pluginManager.PluginAdded += (s, e) =>
            {
                var pluginContextDto = CreatePluginDto(e);
                ImmutableInterlocked.Update(ref _plugins, p => p.Add(pluginContextDto));
                PluginAdded?.Invoke(this, pluginContextDto);
            };
            _pluginManager.PluginRemoved += (s, e) =>
            {
                var pluginContextDto = _plugins.First(p => PluginMetadata.Match(p.Metadata, e.Metadata));
                ImmutableInterlocked.Update(ref _plugins, p => p.Remove(pluginContextDto));
                PluginRemoved?.Invoke(this, pluginContextDto);
            };

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

            var appdataDir = new DirectoryInfo(_appInfo.AppDataDirectory);
            if (!appdataDir.Exists)
            {
                appdataDir.Create();
                Log.Write("Settings", $"Created OpenTabletDriver application data directory: {appdataDir.FullName}");
            }

            var internalPlugins = CreateInternalsPluginContextDto(_pluginManager);
            ImmutableInterlocked.Update(ref _plugins, p => p.Add(internalPlugins));

            _pluginManager.Load();
            _settingsManager.Load(new FileInfo(_appInfo.SettingsFile));

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

        public async Task<IEnumerable<PluginMetadata>> GetInstallablePlugins(string owner, string name, string gitRef)
        {
            return await _pluginManager.GetInstallablePlugins(owner, name, gitRef);
        }

        public Task<IEnumerable<PluginContextDto>> GetPlugins()
        {
            return Task.FromResult<IEnumerable<PluginContextDto>>(_plugins);
        }

        public async Task<bool> InstallPluginFromLocal(string filePath)
        {
            return await _pluginManager.InstallFromLocal(filePath);
        }

        public async Task<bool> InstallPluginFromRemote(PluginMetadata metadata)
        {
            return await _pluginManager.InstallFromRemote(metadata);
        }

        public Task<bool> UninstallPlugin(string pluginName)
        {
            var context = _pluginManager.Plugins.First(ctx => ctx.Name == pluginName);
            return Task.FromResult(_pluginManager.UninstallPlugin(context));
        }

        public Task<IEnumerable<DeviceEndpointDto>> GetDevices()
        {
            return Task.FromResult(_deviceHub.GetDevices().Select(d => new DeviceEndpointDto(d)));
        }

        public Task<IEnumerable<DisplayDto>> GetDisplays()
        {
            var displays = _serviceProvider.GetRequiredService<IVirtualScreen>().Displays
                .Where(t => t is not IVirtualScreen)
                .Select(t => new DisplayDto(t));

            return Task.FromResult(displays);
        }

        public Task DetectTablets()
        {
            _isScanningDevices = true;
            _driver.ScanDevices();

            foreach (var tablet in _driver.InputDevices)
            {
                var profile = GetProfile(tablet.Id);
                ApplyTabletSettings(tablet, profile);
            }

            _isScanningDevices = false;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<int>> GetTablets()
        {
            return Task.FromResult(_driver.InputDevices.Select(c => c.Id));
        }

        public Task<int> GetTabletPersistentId(int tabletId)
        {
            var tablet = GetTablet(tabletId);
            return Task.FromResult(tablet.PersistentId);
        }

        public Task<InputDeviceState> GetTabletState(int tabletId)
        {
            var tablet = GetTablet(tabletId);
            return Task.FromResult(tablet.State);
        }

        public Task SetTabletState(int tabletId, InputDeviceState state)
        {
            var tablet = GetTablet(tabletId);

            switch (state)
            {
                case InputDeviceState.Uninitialized:
                    tablet.Initialize(false);
                    break;
                case InputDeviceState.Normal:
                    tablet.Initialize(true);
                    break;
                default:
                    // noop
                    break;
            }

            return Task.CompletedTask;
        }

        public Task<TabletConfiguration> GetTabletConfiguration(int tabletId)
        {
            var tablet = GetTablet(tabletId);
            return Task.FromResult(tablet.Configuration);
        }

        public Task<Profile> GetTabletProfile(int tabletId)
        {
            var profile = GetProfile(tabletId);
            return Task.FromResult(profile);
        }

        public Task SetTabletProfile(int tabletId, Profile profile)
        {
            ref var pair = ref CollectionsMarshal.GetValueRefOrNullRef(_tablets, tabletId);
            if (Unsafe.IsNullRef(ref pair))
                throw new KeyNotFoundException($"Tablet with ID {tabletId} was not found");

            pair = (pair.Item1, profile);

            ApplyTabletSettings(pair.Item1, profile);
            return Task.CompletedTask;
        }

        public Task ResetTabletProfile(int tabletId)
        {
            if (!_tablets.TryGetValue(tabletId, out var pair))
                throw new KeyNotFoundException($"Tablet with ID {tabletId} was not found");

            var profile = GetDefaultProfile(_serviceProvider, pair.Item1);
            ApplyTabletSettings(pair.Item1, profile);
            TabletProfileChanged?.Invoke(this, new TabletProperty<Profile>(tabletId, profile));
            return Task.CompletedTask;
        }

        public Task<IEnumerable<PluginSettings>> GetToolSettings()
        {
            return Task.FromResult<IEnumerable<PluginSettings>>(_toolSettings);
        }

        public Task SetToolSettings(IEnumerable<PluginSettings> settings)
        {
            ApplyToolSettings(settings);
            ToolsChanged?.Invoke(this, _toolSettings);
            return Task.CompletedTask;
        }

        public Task ResetToolSettings()
        {
            ApplyToolSettings(Enumerable.Empty<PluginSettings>());
            ToolsChanged?.Invoke(this, _toolSettings);
            return Task.CompletedTask;
        }

        public Task SaveSettings()
        {
            Settings settings = CreateSerializableSettings();
            _settingsManager.Save(settings, new FileInfo(_appInfo.SettingsFile));
            settings.Serialize(new FileInfo(_appInfo.SettingsFile));
            Log.Write("Settings", $"Settings saved to '{_appInfo.SettingsFile}'");

            return Task.CompletedTask;
        }

        public Task ResetSettings()
        {
            foreach (var tablet in _driver.InputDevices)
            {
                var profile = GetDefaultProfile(_serviceProvider, tablet);
                ApplyTabletSettings(tablet, profile);
            }

            ApplyToolSettings(Enumerable.Empty<PluginSettings>());
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetPresets()
        {
            return Task.FromResult<IEnumerable<string>>(_presetManager.GetPresets());
        }

        public Task ApplyPreset(string name)
        {
            if (_presetManager.LoadPreset(name) is Preset preset)
            {
                foreach (var tablet in _driver.InputDevices)
                {
                    var profile = preset.Settings.GetProfile(tablet);
                    if (profile is not null)
                        ApplyTabletSettings(tablet, profile);
                }

                ApplyToolSettings(preset.Settings.Tools);
            }
            else
            {
                Log.Write("Presets", $"Unable apply preset \"{name}\" as it does not exist.", LogLevel.Error);
            }

            return Task.CompletedTask;
        }

        public Task SaveAsPreset(string name)
        {
            var settings = CreateSerializableSettings();
            _presetManager.Save(name, settings);
            return Task.CompletedTask;
        }

        public Task<AppInfo> GetApplicationInfo()
        {
            return Task.FromResult(_appInfo);
        }

        public async Task<DiagnosticInfo> GetDiagnostics()
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

        public Task<string?> RequestDeviceString(int tabletId, int index)
        {
            var tablet = GetTablet(tabletId);
            return Task.FromResult(tablet.Digitizer.Endpoint.GetDeviceString((byte)index));
        }

        public Task<string?> RequestDeviceString(int vid, int pid, int index)
        {
            var tablet = _deviceHub.GetDevices().FirstOrDefault(d => d.VendorID == vid && d.ProductID == pid);
            if (tablet == null)
                throw new IOException($"Device not found ({vid:X2}:{pid:X2})");

            return Task.FromResult(tablet.GetDeviceString((byte)index));
        }

        public Task WriteMessage(LogMessage message)
        {
            Log.Write(message);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<LogMessage>> GetCurrentLog()
        {
            return Task.FromResult((IEnumerable<LogMessage>)_logMessages);
        }

        public async Task<UpdateInfoDto?> CheckForUpdates()
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

        private Settings CreateSerializableSettings()
        {
            var tabletProfiles = _tablets.Values
                .Select(t => t.Item2)
                .OrderBy(p => p.Tablet)
                .ThenBy(p => p.PersistentId)
                .ToImmutableArray();

            var settings = new Settings(Settings.CurrentRevision, tabletProfiles, _toolSettings);
            return settings;
        }

        private InputDevice GetTablet(int tabletId)
        {
            if (!_tablets.TryGetValue(tabletId, out var kvp))
                throw new ArgumentException("Tablet not found", nameof(tabletId));

            return kvp.Item1;
        }

        private Profile GetProfile(int tabletId)
        {
            if (!_tablets.TryGetValue(tabletId, out var kvp))
                throw new ArgumentException("Tablet not found", nameof(tabletId));

            return kvp.Item2;
        }

        private void ApplyTabletSettings(InputDevice inputDevice, Profile profile)
        {
            _tablets[inputDevice.Id] = (inputDevice, profile);
            var persistentName = inputDevice.PersistentName;
            Log.Write("Daemon", $"Applying settings for {persistentName}...");
            foreach (var element in inputDevice.OutputMode?.Elements ?? Enumerable.Empty<IDevicePipelineElement>())
            {
                if (element is IDisposable disposable)
                    disposable.Dispose();
            }

            var outputMode = _pluginFactory.Construct<IOutputMode>(profile.OutputMode, inputDevice);

            if (outputMode is not null)
            {
                var outputModeName = _pluginFactory.GetName(profile.OutputMode);
                Log.Write("Settings", $"Output mode: {outputModeName}", LogLevel.Debug);

                var mouseButtonHandler = (outputMode as IMouseButtonSource)?.MouseButtonHandler;

                var deps = new object?[]
                {
                    inputDevice,
                    outputMode,
                    profile.Bindings,
                    mouseButtonHandler
                }.Where(o => o != null).ToArray() as object[];

                var bindingHandler = _serviceProvider.CreateInstance<BindingHandler>(deps);

                ApplyOutputModeSettings(inputDevice, outputMode, profile, bindingHandler);
            }

            // set the output mode atomically
            inputDevice.OutputMode = outputMode;
            TabletProfileChanged?.Invoke(this, new TabletProperty<Profile>(inputDevice.Id, profile));
            Log.Write("Daemon", $"Settings applied");
        }

        private void ApplyOutputModeSettings(InputDevice dev, IOutputMode outputMode, Profile profile, BindingHandler bindingHandler)
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

        private void ApplyToolSettings(IEnumerable<PluginSettings> toolSettings)
        {
            _toolSettings.Clear();

            foreach (var settings in toolSettings)
                _toolSettings.Add(settings);

            foreach (var runningTool in _tools)
                runningTool.Dispose();
            _tools.Clear();

            foreach (var settings in toolSettings)
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

        private static void InitializePlatform()
        {
            switch (SystemInterop.CurrentPlatform)
            {
                case SystemPlatform.Windows:
                    System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

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

        private PluginContextDto CreateInternalsPluginContextDto(IPluginManager pluginManager)
        {
            var internalPluginTypes = pluginManager.InternalPluginTypes;
            var otdVer = typeof(DriverDaemon).Assembly.GetName().Version;
            var metadata = new PluginMetadata()
            {
                Name = "OpenTabletDriver Core",
                Owner = "OpenTabletDriver",
                Description = "OpenTabletDriver's core plugins.",
                PluginVersion = otdVer,
                SupportedDriverVersion = otdVer,
                LicenseIdentifier = "LGPL-3.0",
            };

            var pluginDtos = internalPluginTypes.Select(t => ExtractDto(t));

            return new PluginContextDto
            {
                Metadata = metadata,
                Plugins = new Collection<PluginDto>(pluginDtos.ToArray())
            };
        }

        private PluginContextDto CreatePluginDto(PluginContext context)
        {
            var metadata = context.Metadata;
            var pluginDtos = context.Assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => _pluginManager.IsLoadablePlugin(t) && t.IsPlatformSupported())
                .Select(t => ExtractDto(t));

            return new PluginContextDto
            {
                Metadata = metadata,
                Plugins = new Collection<PluginDto>(pluginDtos.ToArray())
            };
        }

        private PluginDto ExtractDto(Type pluginType)
        {
            var path = pluginType.FullName!;
            var interfaces = _pluginManager.GetImplementedInterfaces(pluginType).Select(t => t.FullName!).ToImmutableArray();
            var name = pluginType.GetFriendlyName() ?? path;
            var settingsMetadatas = pluginType.GetSettingsMetadatas(_serviceProvider).ToImmutableArray();
            var attributes = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

            return new PluginDto(path, interfaces, name, settingsMetadatas, attributes);
        }

        private Profile GetOrCreateProfile(IServiceProvider serviceProvider, InputDevice inputDevice)
        {
            // find a profile from memory
            var profile = _tablets.FirstOrDefault(kvp =>
            {
                var (tabletId, (tablet, profile)) = kvp;
                return tablet.Configuration.Name == inputDevice.Configuration.Name
                    && tablet.PersistentId == inputDevice.PersistentId;
            }).Value.Item2;

            profile ??= _settingsManager.Settings.Profiles.FirstOrDefault(p => p.Tablet == inputDevice.Configuration.Name && p.PersistentId == inputDevice.PersistentId);
            profile ??= GetDefaultProfile(serviceProvider, inputDevice);

            ref var pair = ref CollectionsMarshal.GetValueRefOrAddDefault(_tablets, inputDevice.Id, out _);
            pair = (inputDevice, profile);

            return profile;
        }

        private static Profile GetDefaultProfile(IServiceProvider serviceProvider, InputDevice inputDevice)
        {
            var screen = serviceProvider.GetRequiredService<IVirtualScreen>();
            var digitizer = inputDevice.Configuration.Specifications.Digitizer;

            return new Profile
            {
                Tablet = inputDevice.Configuration.Name,
                PersistentId = inputDevice.PersistentId,
                OutputMode = serviceProvider.GetDefaultSettings(typeof(AbsoluteMode), digitizer!, screen),
                Bindings = GetDefaultBindings(inputDevice.Configuration.Specifications)
            };
        }

        private static BindingSettings GetDefaultBindings(TabletSpecifications specifications)
        {
            var bindingSettings = new BindingSettings
            {
                TipButton = new PluginSettings(
                    typeof(MouseBinding),
                    new
                    {
                        Button = nameof(MouseButton.Left)
                    }
                ),
                PenButtons = new Collection<PluginSettings?>(),
                AuxButtons = new Collection<PluginSettings?>(),
                MouseButtons = new Collection<PluginSettings?>()
            };
            return bindingSettings;
        }
    }
}
