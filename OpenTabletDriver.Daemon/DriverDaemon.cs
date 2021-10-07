using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Migration;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Interop;
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
        public DriverDaemon(Driver driver)
        {
            Driver = driver;

            Log.Output += (sender, message) =>
            {
                LogMessages.Add(message);
                Console.WriteLine(Log.GetStringFormat(message));
                Message?.Invoke(sender, message);
            };

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
                Log.Write("Detect", $"Another tablet driver found: {driverInfo.Name}", LogLevel.Warning);
                if (driverInfo.IsBlockingDriver)
                    Log.Write("Detect", $"Detection for {driverInfo.Name} tablets might be impaired", LogLevel.Warning);
                else if (driverInfo.IsSendingInput)
                    Log.Write("Detect", $"Detected input coming from {driverInfo.Name} driver", LogLevel.Error);
            }

            LoadUserSettings();

            SleepDetection = new(async () =>
            {
                Log.Write(nameof(SleepDetectionThread), "Sleep detected...", LogLevel.Debug);
                await DetectTablets();
            });

            SleepDetection.Start();
        }

        public event EventHandler<LogMessage>? Message;
        public event EventHandler<DebugReportData>? DeviceReport;
        public event EventHandler<IEnumerable<TabletReference>>? TabletsChanged;

        public Driver Driver { get; }
        private Settings? Settings { set; get; }
        private Collection<LogMessage> LogMessages { set; get; } = new Collection<LogMessage>();
        private Collection<ITool> Tools { set; get; } = new Collection<ITool>();
        private readonly SleepDetectionThread SleepDetection;

        private bool debugging;

        public Task WriteMessage(LogMessage message)
        {
            Log.OnOutput(message);
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
            AppInfo.PluginManager.AddSingleton<IDriver>(this.Driver);

            return Task.CompletedTask;
        }

        public Task<bool> InstallPlugin(string filePath)
        {
            return Task.FromResult(AppInfo.PluginManager.InstallPlugin(filePath));
        }

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
            var configDir = new DirectoryInfo(AppInfo.Current.ConfigurationDirectory);
            if (configDir.Exists)
            {
                Driver.Detect();

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
            else
            {
                Log.Write("Detect", $"The configuration directory '{configDir.FullName}' does not exist.", LogLevel.Error);
            }
            Log.Write("Detect", "No tablet found.");
            return Array.Empty<TabletReference>();
        }

        public Task SetSettings(Settings? settings)
        {
            // Dispose filters that implement IDisposable interface
            foreach (var obj in Driver.InputDevices.SelectMany(d => d.OutputMode?.Elements ?? (IEnumerable<object>)Array.Empty<object>()))
                if (obj is IDisposable disposable)
                    disposable.Dispose();

            Settings = settings ?? Settings.GetDefaults();

            foreach (var dev in Driver.InputDevices)
            {
                string group = dev.Properties.Name;
                var profile = Settings.Profiles[dev];

                profile.BindingSettings.MatchSpecifications(dev.Properties.Specifications);

                var serviceCollection = AppInfo.PluginManager.Clone();
                serviceCollection.AddSingleton(dev.Properties);

                var serviceProvider = serviceCollection.BuildServiceProvider();

                dev.OutputMode = profile.OutputMode.Construct<IOutputMode>(serviceProvider);

                if (dev.OutputMode != null)
                    Log.Write(group, $"Output mode: {profile.OutputMode.Name}");

                if (dev.OutputMode is AbsoluteOutputMode absoluteMode)
                    SetAbsoluteModeSettings(dev, absoluteMode, profile.AbsoluteModeSettings);

                if (dev.OutputMode is RelativeOutputMode relativeMode)
                    SetRelativeModeSettings(dev, relativeMode, profile.RelativeModeSettings);

                if (dev.OutputMode is IOutputMode outputMode)
                {
                    SetOutputModeSettings(serviceCollection, dev, outputMode, profile);
                    SetBindingHandlerSettings(serviceCollection, dev, outputMode, profile.BindingSettings);
                }
            }

            Log.Write("Settings", "Driver is enabled.");

            SetToolSettings(AppInfo.PluginManager);

            return Task.CompletedTask;
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
                SettingsMigrator.Migrate(AppInfo.Current);
                var settings = Settings.Deserialize(settingsFile);
                if (settings != null)
                {
                    await SetSettings(settings);
                }
                else
                {
                    Log.Write("Settings", "Invalid settings detected. Attempting recovery.", LogLevel.Error);
                    settings = Settings.GetDefaults();
                    Settings.Recover(settingsFile, settings);
                    Log.Write("Settings", "Recovery complete");
                    await SetSettings(settings);
                }
            }
            else
            {
                await ResetSettings();
            }
        }

        private void SetOutputModeSettings(IServiceCollection serviceCollection, InputDeviceTree dev, IOutputMode outputMode, Profile profile)
        {
            var provider = serviceCollection.BuildServiceProvider();

            string group = dev.Properties.Name;
            outputMode.Tablet = dev;

            var elements = from store in profile.Filters
                where store.Enable
                let filter = store.Construct<IPositionedPipelineElement<IDeviceReport>>(provider)
                where filter != null
                select filter;
            outputMode.Elements = elements.ToList();

            if (outputMode.Elements != null && outputMode.Elements.Count() > 0)
                Log.Write(group, $"Filters: {string.Join(", ", outputMode.Elements)}");
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

        private void SetBindingHandlerSettings(IServiceCollection serviceCollection, InputDeviceTree dev, IOutputMode outputMode, BindingSettings settings)
        {
            string group = dev.Properties.Name;
            var bindingHandler = new BindingHandler(outputMode);

            var bindingServices = serviceCollection.Clone();
            object? pointer = outputMode switch
            {
                AbsoluteOutputMode absoluteOutputMode => absoluteOutputMode.Pointer,
                RelativeOutputMode relativeOutputMode => relativeOutputMode.Pointer,
                _ => null
            };

            if (pointer is IVirtualMouse virtualMouse)
                bindingServices.AddSingleton(virtualMouse);

            var bindingServiceProvider = bindingServices.BuildServiceProvider();

            var tip = bindingHandler.Tip = new ThresholdBindingState
            {
                Binding = settings.TipButton?.Construct<IBinding>(bindingServiceProvider),
                ActivationThreshold = settings.TipActivationPressure
            };

            if (tip.Binding != null)
            {
                Log.Write(group, $"Tip Binding: [{tip.Binding}]@{tip.ActivationThreshold}%");
            }

            var eraser = bindingHandler.Eraser = new ThresholdBindingState
            {
                Binding = settings.EraserButton?.Construct<IBinding>(bindingServiceProvider),
                ActivationThreshold = settings.EraserActivationPressure
            };

            if (eraser.Binding != null)
            {
                Log.Write(group, $"Eraser Binding: [{eraser.Binding}]@{eraser.ActivationThreshold}%");
            }

            if (settings.PenButtons != null && settings.PenButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(bindingServiceProvider, settings.PenButtons, bindingHandler.PenButtons);
                Log.Write(group, $"Pen Bindings: " + string.Join(", ", bindingHandler.PenButtons.Select(b => b.Value?.Binding)));
            }

            if (settings.AuxButtons != null && settings.AuxButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(bindingServiceProvider, settings.AuxButtons, bindingHandler.AuxButtons);
                Log.Write(group, $"Express Key Bindings: " + string.Join(", ", bindingHandler.AuxButtons.Select(b => b.Value?.Binding)));
            }

            if (settings.MouseButtons != null && settings.MouseButtons.Any(b => b?.Path != null))
            {
                SetBindingHandlerCollectionSettings(bindingServiceProvider, settings.MouseButtons, bindingHandler.MouseButtons);
                Log.Write(group, $"Mouse Button Bindings: [" + string.Join("], [", bindingHandler.MouseButtons.Select(b => b.Value?.Binding)) + "]");
            }

            var scrollUp = bindingHandler.MouseScrollUp = new BindingState
            {
                Binding = settings.MouseScrollUp?.Construct<IBinding>(bindingServiceProvider)
            };

            var scrollDown = bindingHandler.MouseScrollDown = new BindingState
            {
                Binding = settings.MouseScrollDown?.Construct<IBinding>(bindingServiceProvider)
            };

            if (scrollUp.Binding != null || scrollDown.Binding != null)
            {
                Log.Write(group, $"Mouse Scroll: Up: [{scrollUp?.Binding}] Down: [{scrollDown?.Binding}]");
            }
        }

        private void SetBindingHandlerCollectionSettings(IServiceProvider serviceProvider, PluginSettingStoreCollection collection, Dictionary<int, BindingState?> targetDict)
        {
            for (int index = 0; index < collection.Count; index++)
            {
                IBinding? binding = collection[index]?.Construct<IBinding>(serviceProvider);
                var state = binding == null ? null : new BindingState
                {
                    Binding = binding
                };

                if(!targetDict.TryAdd(index, state))
                    targetDict[index] = state;
            }
        }

        private void SetToolSettings(IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();

            foreach (var runningTool in Tools)
                runningTool.Dispose();
            Tools.Clear();

            if (Settings != null)
            {
                foreach (PluginSettingStore store in Settings.Tools)
                {
                    if (store.Enable == false)
                        continue;

                    var tool = store.Construct<ITool>(serviceProvider);

                    if (tool?.Initialize() ?? false)
                        Tools.Add(tool);
                    else
                        Log.Write("Tool", $"Failed to initialize {store.Name} tool.", LogLevel.Error);
                }
            }
        }

        public Task<Settings?> GetSettings()
        {
            return Task.FromResult(Settings);
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
            return Task.FromResult((IEnumerable<LogMessage>)LogMessages);
        }

        private void PostDebugReport(TabletReference tablet, IDeviceReport report)
        {
            if (report != null && tablet != null)
                DeviceReport?.Invoke(this, new DebugReportData(tablet, report));
        }
    }
}
