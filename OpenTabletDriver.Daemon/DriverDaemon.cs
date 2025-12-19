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
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.SystemDrivers;

#nullable enable

namespace OpenTabletDriver.Daemon
{
    public class DriverDaemon : IDriverDaemon
    {
        private const string AVALONIA_REVISION = "0.7.0.0";

        public DriverDaemon(Driver driver)
        {
            Driver = driver;
            _logFile = new LogFile(AppInfo.Current.LogDirectory);

            Log.Output += (sender, message) =>
            {
                _logFile.Write(message);
                Message?.Invoke(sender, message);
            };

            InitializePlatform();
            Driver.TabletsChanged += (sender, e) => TabletsChanged?.Invoke(sender, e);
            Driver.CompositeDeviceHub.DevicesChanged += async (sender, args) =>
            {
                if (args.Additions.Any())
                {
                    await DetectTablets();
                    await SetSettings(Settings);
                }
            };

            foreach (var driverInfo in DriverInfo.GetDriverInfos())
            {
                var os = SystemInterop.CurrentPlatform switch
                {
                    PluginPlatform.Windows => "Windows",
                    PluginPlatform.Linux => "Linux",
                    PluginPlatform.MacOS => "MacOS",
                    _ => null
                };
                var wikiUrl = $"https://opentabletdriver.net/Wiki/FAQ/{os}";

                var message = new StringBuilder();
                message.Append($"'{driverInfo.Name}' driver is detected.");

                if (driverInfo.Status.HasFlag(DriverStatus.Blocking))
                    message.Append(" It will block detection of tablets.");
                if (driverInfo.Status.HasFlag(DriverStatus.Flaky))
                    message.Append(" It will cause flaky support to tablets.");
                if (driverInfo.Status.HasFlag(DriverStatus.Uncertain))
                    message.Append(" It may be a false positive.");

                var processStrings = safeGetProcessDetails(driverInfo.Processes);
                message.Append($" Processes found: [" + string.Join(", ", processStrings) + "].");

                if (os != null)
                    message.Append($" If any problems arise, visit '{wikiUrl}'.");

                Log.WriteNotify("Detect", message.ToString(), LogLevel.Warning);
            }

            LoadUserSettings();

            SleepDetector.Slept += async () =>
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    return;

                Log.Write(nameof(DriverDaemon), "Sleep detected...", LogLevel.Info);
                await DetectTablets();
                await SetSettings(Settings);
            };
        }

        private IEnumerable<string> safeGetProcessDetails(Process[] processes)
        {
            foreach (var driverProcess in processes)
            {
                var details = "";
                try
                {
                    details += driverProcess.ProcessName;
                }
                catch
                {
                    details += "Failed to get ProcessName";
                }

                try
                {
                    details += ": " + driverProcess.MainModule?.FileName;
                }
                catch
                {
                    details += ": Failed to get FileName";
                }
                yield return details;
            }
        }

        public event EventHandler<LogMessage>? Message;
        public event EventHandler<DebugReportData>? DeviceReport;
        public event EventHandler<IEnumerable<TabletReference>>? TabletsChanged;
        public event EventHandler? Resynchronize;

        public Driver Driver { get; }
        private Settings? Settings { set; get; }
        private Collection<ITool> Tools { set; get; } = new Collection<ITool>();
        private IUpdater Updater = DesktopInterop.Updater;
        private readonly ISleepDetector? SleepDetector = new SleepDetector();
        private Settings? lastValidSettings;

        private UpdateInfo? _updateInfo;
        private LogFile _logFile;

        private bool debugging;

        public Task WriteMessage(LogMessage message)
        {
            Log.Write(message);
            return Task.CompletedTask;
        }

        public Task LoadPlugins()
        {
            var pluginDir = new DirectoryInfo(AppInfo.Current.PluginDirectory);

            if (!pluginDir.Exists)
            {
                pluginDir.Create();
                Log.Write("Plugin", $"The plugin directory '{pluginDir.FullName}' has been created");
            }

            AppInfo.PluginManager.Load();

            // Add services to inject on plugin construction
            AppInfo.PluginManager.AddService<IDriver>(() => this.Driver);
            AppInfo.PluginManager.AddService<IDriverDaemon>(() => this);

            return Task.CompletedTask;
        }

        public Task<bool> InstallPlugin(string filePath)
        {
            return Task.FromResult(AppInfo.PluginManager.InstallPlugin(filePath));
        }

        // FIXME: needs API bump: IDriverDaemon expects friendlyName but this implementation takes a full path
        public Task<bool> UninstallPlugin(string directoryPath)
        {
            var plugins = AppInfo.PluginManager.GetLoadedPlugins();
            var context = plugins.First(ctx => ctx.Directory.FullName == directoryPath);
            return Task.FromResult(AppInfo.PluginManager.UninstallPlugin(context));
        }

        public Task<bool> DownloadPlugin(PluginMetadata metadata)
        {
            return AppInfo.PluginManager.DownloadPlugin(metadata);
        }

        public Task<IEnumerable<TabletReference>> GetTablets()
        {
            return Task.FromResult(Driver.Tablets);
        }

        public async Task<IEnumerable<TabletReference>> DetectTablets()
        {
            Driver.Detect();
            await Task.Run(CheckForProblematicProcesses).ConfigureAwait(false);

            foreach (var tablet in Driver.InputDevices)
            {
                foreach (var dev in tablet.InputDevices)
                {
                    dev.RawReport += (_, report) => PostDebugReport(tablet, report);
                    dev.RawClone = debugging;
                }
            }

            return await GetTablets();
        }

        public Task SetSettings(Settings? settings)
        {
            try
            {
                foreach (var dev in Driver.InputDevices)
                    dev.OutputMode?.Dispose();

                Settings = settings ??= Settings.GetDefaults();

                foreach (InputDeviceTree? dev in Driver.InputDevices)
                {
                    var tabletReference = dev.CreateReference();
                    string group = dev.Properties.Name;
                    var profile = Settings.Profiles[dev];

                    profile.BindingSettings.MatchSpecifications(dev.Properties.Specifications);

                    dev.OutputMode = profile.OutputMode.Construct<IOutputMode>(tabletReference);

                    if (dev.OutputMode != null)
                        Log.Write(group, $"Output mode: {profile.OutputMode.Name}");

                    if (dev.OutputMode is AbsoluteOutputMode absoluteMode)
                    {
                        SetAbsoluteModeSettings(dev, absoluteMode, profile.AbsoluteModeSettings);
                        if (absoluteMode.Pointer is IPressureHandler)
                            LogPressureState(group, profile);
                        if (absoluteMode.Pointer is ITiltHandler)
                            LogTiltState(group, profile);
                        if (absoluteMode.Pointer is IConfidenceHandler)
                            LogReportConfidence(group, profile);
                    }

                    if (dev.OutputMode is RelativeOutputMode relativeMode)
                    {
                        SetRelativeModeSettings(dev, relativeMode, profile.RelativeModeSettings);
                        if (relativeMode.Pointer is IPressureHandler)
                            LogPressureState(group, profile);
                        if (relativeMode.Pointer is ITiltHandler)
                            LogTiltState(group, profile);
                        if (relativeMode.Pointer is IConfidenceHandler)
                            LogReportConfidence(group, profile);
                    }

                    if (dev.OutputMode is { } outputMode)
                    {
                        outputMode.Tablet = tabletReference;
                        var bindingHandler = CreateBindingHandler(dev, outputMode, profile.BindingSettings);
                        SetOutputModeElements(dev, outputMode, profile, bindingHandler);

                        outputMode.DisablePressure = profile.BindingSettings.DisablePressure;
                        outputMode.DisableTilt = profile.BindingSettings.DisableTilt;
                        outputMode.ConfidentReportsOnly = profile.BindingSettings.ConfidentReportsOnly;
                    }
                }

                if (Driver.InputDevices.Length > 0)
                    Log.Write("Settings", "Driver is enabled.");

                SetToolSettings();

                lastValidSettings = settings;
                return Task.CompletedTask;
            }
            catch
            {
                try
                {
                    SetSettings(lastValidSettings);
                    Log.Write("Settings", "Failed to apply settings. Reverted to last valid settings.", LogLevel.Error, true);
                }
                catch
                {
                    RecoverSettings(settings);
                    Log.Write("Settings", "Failed to apply settings. Attempted recovery. Some settings may have been lost.", LogLevel.Error, true);
                }

                Resynchronize?.Invoke(this, EventArgs.Empty);
                return Task.CompletedTask;
            }
        }

        private static void LogTiltState(string group, Profile profile)
        {
            Log.Write(group, $"Tilt: {(profile.BindingSettings.DisableTilt ? "Disabled" : "Enabled")}");
        }

        private static void LogPressureState(string group, Profile profile)
        {
            Log.Write(group,
                $"Pressure: {(profile.BindingSettings.DisablePressure ? "Disabled" : "Enabled")}");
        }

        private static void LogReportConfidence(string group, Profile profile)
        {
            Log.Write(group,
                $"Confident reports only: {(profile.BindingSettings.ConfidentReportsOnly ? "Enabled" : "Disabled")}");
        }

        private void RecoverSettings(Settings? settings)
        {
            var recoveredSettings = Settings.GetDefaults();

            if (settings != null)
            {
                // Copy by value, not by reference
                foreach (var profile in settings.Profiles)
                {
                    var recoveredProfile = recoveredSettings.Profiles.GetProfile(profile.Tablet);
                    if (recoveredProfile != null)
                    {
                        recoveredProfile.AbsoluteModeSettings = new AbsoluteModeSettings
                        {
                            Display = new AreaSettings
                            {
                                Area = profile.AbsoluteModeSettings.Display.Area
                            },
                            Tablet = new AreaSettings
                            {
                                Area = profile.AbsoluteModeSettings.Tablet.Area
                            },
                            EnableClipping = profile.AbsoluteModeSettings.EnableClipping,
                            EnableAreaLimiting = profile.AbsoluteModeSettings.EnableAreaLimiting,
                            LockAspectRatio = profile.AbsoluteModeSettings.LockAspectRatio
                        };
                        recoveredProfile.RelativeModeSettings = new RelativeModeSettings
                        {
                            Sensitivity = profile.RelativeModeSettings.Sensitivity,
                            RelativeRotation = profile.RelativeModeSettings.RelativeRotation,
                            ResetTime = profile.RelativeModeSettings.ResetTime
                        };
                    }
                }
            }

            SetSettings(recoveredSettings);
        }

        public async Task ResetSettings()
        {
            await SetSettings(Settings.GetDefaults());
        }

        private async void LoadUserSettings()
        {
            AppInfo.PluginManager.Clean();
            await LoadPlugins();
            await DetectTablets();

            var appdataDir = new DirectoryInfo(AppInfo.Current.AppDataDirectory);
            if (!appdataDir.Exists)
            {
                appdataDir.Create();
                Log.Write("Settings", $"Created OpenTabletDriver application data directory: {appdataDir.FullName}");
            }

            var settingsFile = new FileInfo(AppInfo.Current.SettingsFile);

            if (settingsFile.Exists)
            {
                if (Settings.TryDeserialize(settingsFile, out var settings) &&
                    settings.Revision != AVALONIA_REVISION)
                {
                    settings.Revision = Settings.GetVersion(); // ensure Revision matches Daemon version
                    await SetSettings(settings);
                }
                else
                {
                    Log.Write("Settings", "Invalid settings found. Moving invalid config.");
                    MoveSettingsFile();
                    await ResetSettings();
                }
            }
            else
            {
                await ResetSettings();

                // only save fresh settings if a tablet was configured
                if (Settings!.Profiles.Any())
                    Settings.Serialize(settingsFile);
            }
        }

        private void MoveSettingsFile()
        {
            var src = AppInfo.Current.SettingsFile;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var dstFileName = $"settings_bak-{now}.json";
            var dst = Path.Join(AppInfo.Current.AppDataDirectory, dstFileName);

            Log.Write("MoveSettingsFile", $"Moving settings file at '{src}' to '{dst}'", LogLevel.Debug);
            File.Move(src, dst);
        }

        private void SetOutputModeElements(InputDeviceTree dev, IOutputMode outputMode, Profile profile, BindingHandler bindingHandler)
        {
            string group = dev.Properties.Name;

            var elements = from store in profile.Filters
                           where store.Enable
                           let filter = store.Construct<IPositionedPipelineElement<IDeviceReport>>(outputMode.Tablet)
                           where filter != null
                           select filter;
            outputMode.Elements = elements.Append(bindingHandler).ToList();

            var activeFilters = outputMode.Elements.Where(e => e != bindingHandler).ToList();
            if (activeFilters.Count != 0)
                Log.Write(group, $"Filters: {string.Join(", ", activeFilters)}");
        }

        private void SetAbsoluteModeSettings(InputDeviceTree dev, AbsoluteOutputMode absoluteMode, AbsoluteModeSettings settings)
        {
            string group = dev.Properties.Name;
            absoluteMode.Output = settings.Display.Area;

            Log.Write(group, $"Display area: {absoluteMode.Output}");

            absoluteMode.Input = settings.Tablet.Area;
            Log.Write(group, $"Tablet area: {absoluteMode.Input}");

            absoluteMode.AreaClipping = settings.EnableClipping;
            Log.Write(group, $"Clipping: {(absoluteMode.AreaClipping ? "Enabled" : "Disabled")}");

            absoluteMode.AreaLimiting = settings.EnableAreaLimiting;
            Log.Write(group, $"Ignoring reports outside area: {(absoluteMode.AreaLimiting ? "Enabled" : "Disabled")}");
        }

        private void SetRelativeModeSettings(InputDeviceTree dev, RelativeOutputMode relativeMode, RelativeModeSettings settings)
        {
            string group = dev.Properties.Name;
            relativeMode.Sensitivity = settings.Sensitivity;

            Log.Write(group, $"Relative Mode Sensitivity (X, Y): {relativeMode.Sensitivity}");

            relativeMode.Rotation = settings.RelativeRotation;
            Log.Write(group, $"Relative Mode Rotation: {relativeMode.Rotation}");

            relativeMode.ResetTime = settings.ResetTime;
            Log.Write(group, $"Reset time: {relativeMode.ResetTime}");
        }

        /// <summary>
        /// Checks for any problematic processes running on the user's computer that may
        /// impair function or detection of tablets, such as video game anti-cheat software.
        /// </summary>
        private void CheckForProblematicProcesses()
        {
            if (SystemInterop.CurrentPlatform == PluginPlatform.Windows)
            {
                if (Process.GetProcessesByName("vgc").Any())
                    Log.Write("Detect", "Valorant's anti-cheat program Vanguard is detected. Tablet function may be impaired.", LogLevel.Warning);
                if (Process.GetProcessesByName("VALORANT-Win64-Shipping").Any())
                    Log.Write("Detect", "Valorant is detected. Tablet function may be impaired.", LogLevel.Warning);
            }
        }

        private static BindingHandler CreateBindingHandler(InputDeviceTree dev, IOutputMode outputMode, BindingSettings settings)
        {
            string group = dev.Properties.Name;
            var tabletReference = outputMode.Tablet;
            var bindingHandler = new BindingHandler(tabletReference);

            var bindingServiceProvider = new ServiceManager();
            object? pointer = outputMode switch
            {
                AbsoluteOutputMode absoluteOutputMode => absoluteOutputMode.Pointer,
                RelativeOutputMode relativeOutputMode => relativeOutputMode.Pointer,
                _ => null
            };

            if (pointer is IMouseButtonHandler mouseButtonHandler)
                bindingServiceProvider.AddService(() => mouseButtonHandler);

            if (pointer is IMouseScrollHandler mouseScrollHandler)
                bindingServiceProvider.AddService(() => mouseScrollHandler);

            if (pointer is IPenActionHandler penActionHandler)
                bindingServiceProvider.AddService(() => penActionHandler);

            var tip = bindingHandler.Tip = new ThresholdBindingState
            {
                Binding = settings.TipButton?.Construct<IBinding>(bindingServiceProvider, tabletReference),

                ActivationThreshold = settings.TipActivationThreshold
            };

            if (tip.Binding != null)
            {
                Log.Write(group, $"Tip Binding: [{tip.Binding}]@{tip.ActivationThreshold}%");
            }

            var eraser = bindingHandler.Eraser = new ThresholdBindingState
            {
                Binding = settings.EraserButton?.Construct<IBinding>(bindingServiceProvider, tabletReference),
                ActivationThreshold = settings.EraserActivationThreshold
            };

            if (eraser.Binding != null)
            {
                Log.Write(group, $"Eraser Binding: [{eraser.Binding}]@{eraser.ActivationThreshold}%");
            }

            if (settings.PenButtons != null && settings.PenButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(bindingServiceProvider, settings.PenButtons, bindingHandler.PenButtons, tabletReference);
                Log.Write(group, $"Pen Bindings: " + string.Join(", ", bindingHandler.PenButtons.Select(b => b.Value?.Binding)));
            }

            if (settings.AuxButtons != null && settings.AuxButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(bindingServiceProvider, settings.AuxButtons, bindingHandler.AuxButtons, tabletReference);
                Log.Write(group, $"Express Key Bindings: " + string.Join(", ", bindingHandler.AuxButtons.Select(b => b.Value?.Binding)));
            }

            if (settings.MouseButtons != null && settings.MouseButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(bindingServiceProvider, settings.MouseButtons, bindingHandler.MouseButtons, tabletReference);
                Log.Write(group, $"Mouse Button Bindings: [" + string.Join("], [", bindingHandler.MouseButtons.Select(b => b.Value?.Binding)) + "]");
            }

            if (settings.WheelButtons != null && settings.WheelButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(bindingServiceProvider, settings.WheelButtons, bindingHandler.WheelButtons, tabletReference);
                Log.Write(group, $"Wheel Button Bindings: [" + string.Join("], [", bindingHandler.WheelButtons.Select(b => b.Value?.Binding)) + "]");
            }

            var scrollUp = bindingHandler.MouseScrollUp = new BindingState
            {
                Binding = settings.MouseScrollUp?.Construct<IBinding>(bindingServiceProvider, tabletReference)
            };

            var scrollDown = bindingHandler.MouseScrollDown = new BindingState
            {
                Binding = settings.MouseScrollDown?.Construct<IBinding>(bindingServiceProvider, tabletReference)
            };

            if (scrollUp.Binding != null || scrollDown.Binding != null)
            {
                Log.Write(group, $"Mouse Scroll: Up: [{scrollUp?.Binding}] Down: [{scrollDown?.Binding}]");
            }

            var clockwiseRotation = bindingHandler.ClockwiseRotation = new DeltaThresholdBindingState
            {
                Binding = settings.ClockwiseRotation?.Construct<IBinding>(bindingServiceProvider, tabletReference),
                ActivationThreshold = settings.ClockwiseActivationThreshold
                    * (tabletReference.Properties.Specifications.Wheel?.StepCount ?? 1) / 100,
                IsNegativeThreshold = false
            };

            var counterClockwiseRotation = bindingHandler.CounterClockwiseRotation = new DeltaThresholdBindingState
            {
                Binding = settings.CounterClockwiseRotation?.Construct<IBinding>(bindingServiceProvider, tabletReference),
                ActivationThreshold = -settings.CounterClockwiseActivationThreshold
                    * (tabletReference.Properties.Specifications.Wheel?.StepCount ?? 1) / 100,
                IsNegativeThreshold = true
            };

            if (clockwiseRotation.Binding != null || counterClockwiseRotation.Binding != null)
            {
                Log.Write(group, $"Wheel: Clockwise Rotation: [{clockwiseRotation?.Binding}] Counter-Clockwise Rotation: [{counterClockwiseRotation?.Binding}]");
            }

            return bindingHandler;
        }

        private static void SetBindingHandlerCollectionSettings(IServiceManager serviceManager, PluginSettingStoreCollection collection, Dictionary<int, BindingState?> targetDict, TabletReference tabletReference)
        {
            for (int index = 0; index < collection.Count; index++)
            {
                var binding = collection[index]?.Construct<IBinding>(serviceManager, tabletReference);
                var state = binding == null ? null : new BindingState
                {
                    Binding = binding
                };

                if (!targetDict.TryAdd(index, state))
                    targetDict[index] = state;
            }
        }

        private static void SetBindingHandlerRangeCollectionSettings(IServiceManager serviceManager, PluginSettingStoreCollection collection, float[] ends, Dictionary<int, RangeBindingState?> targetDict, TabletReference tabletReference)
        {
            var start = 0;

            for (int index = 0; index < collection.Count; index++)
            {
                var binding = collection[index]?.Construct<IBinding>(serviceManager, tabletReference);
                var end = ends[index];
                var state = binding == null ? null : new RangeBindingState
                {
                    Binding = binding,
                    StartThreshold = start,
                    EndThreshold = end >= start ? end : start
                };

                if (!targetDict.TryAdd(index, state))
                    targetDict[index] = state;
            }
        }

        private void SetToolSettings()
        {
            foreach (var runningTool in Tools)
                runningTool.Dispose();
            Tools.Clear();

            if (Settings != null)
            {
                foreach (PluginSettingStore store in Settings.Tools)
                {
                    if (store.Enable == false)
                        continue;

                    var tool = store.Construct<ITool>();

                    if (tool?.Initialize() ?? false)
                        Tools.Add(tool);
                    else
                        Log.Write("Tool", $"Failed to initialize {store.Name} tool.", LogLevel.Error);
                }
            }
        }

        public Task<Settings> GetSettings()
        {
            return Task.FromResult(Settings!);
        }

        public Task<IEnumerable<SerializedDeviceEndpoint>> GetDevices()
        {
            return Task.FromResult(Driver.CompositeDeviceHub.GetDevices().Select(d => new SerializedDeviceEndpoint(d)));
        }

        public Task<AppInfo> GetApplicationInfo()
        {
            return Task.FromResult(AppInfo.Current);
        }

        public Task SetTabletDebug(bool enabled)
        {
            debugging = enabled;
            foreach (var dev in Driver.InputDevices.SelectMany(d => d.InputDevices))
                dev.RawClone = debugging;

            Log.Debug("Tablet", $"Tablet debugging is {(debugging ? "enabled" : "disabled")}");

            return Task.CompletedTask;
        }

        public Task<string> RequestDeviceString(int vid, int pid, int index)
        {
            var tablet = Driver.CompositeDeviceHub.GetDevices().Where(d => d.VendorID == vid && d.ProductID == pid).FirstOrDefault();
            if (tablet == null)
                throw new IOException("Device not found");

            return Task.FromResult(tablet.GetDeviceString((byte)index));
        }

        public Task<IEnumerable<LogMessage>> GetCurrentLog()
        {
            return Task.FromResult(_logFile.Read());
        }

        private void PostDebugReport(TabletReference tablet, IDeviceReport report)
        {
            if (report != null && tablet != null)
                DeviceReport?.Invoke(this, new DebugReportData(tablet, report));
        }

        public async Task<SerializedUpdateInfo?> CheckForUpdates()
        {
            if (Updater == null)
                return null;

            _updateInfo = await Updater.CheckForUpdates();
            return _updateInfo?.ToSerializedUpdateInfo();
        }

        public async Task InstallUpdate()
        {
            if (_updateInfo == null)
                throw new InvalidOperationException("No update available"); // Misbehaving client

            try
            {
                var update = await _updateInfo.GetUpdate();
                Updater?.Install(update);
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

        public Task ForceResynchronize()
        {
            Resynchronize?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private static void InitializePlatform()
        {
            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.Windows:
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

                    if (Environment.OSVersion.Version.Build >= 22000) // Windows 11
                    {
                        unsafe
                        {
                            var state = Windows.PowerThrottlingState.Create();
                            state.ControlMask = (int)Windows.PowerThrottlingStateMask.IgnoreTimerResolution;

                            if (!Windows.SetProcessInformation(
                                Process.GetCurrentProcess().Handle,
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
