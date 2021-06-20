using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HidSharp;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Migration;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Daemon
{
    public class DriverDaemon : IDriverDaemon
    {
        public DriverDaemon()
        {
            Log.Output += (sender, message) =>
            {
                LogMessages.Add(message);
                Console.WriteLine(Log.GetStringFormat(message));
                Message?.Invoke(sender, message);
            };
            Driver.TabletsChanged += (sender, e) => TabletsChanged?.Invoke(sender, e);
            HidSharpDeviceRootHub.Current.DevicesChanged += async (sender, args) =>
            {
                if (args.Additions.Any())
                {
                    await DetectTablets();
                    await SetSettings(Settings);
                }
            };

            LoadUserSettings();
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

        public event EventHandler<LogMessage> Message;
        public event EventHandler<DebugReportData> DeviceReport;
        public event EventHandler<IEnumerable<TabletReference>> TabletsChanged;

        public DesktopDriver Driver { private set; get; } = new DesktopDriver();
        private Settings Settings { set; get; }
        private Collection<LogMessage> LogMessages { set; get; } = new Collection<LogMessage>();
        private Collection<ITool> Tools { set; get; } = new Collection<ITool>();

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
            AppInfo.PluginManager.AddService<IDriver>(() => this.Driver);

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

                foreach (var tablet in Driver.Devices)
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
            return null;
        }

        public Task SetSettings(Settings settings)
        {
            // Dispose filters that implement IDisposable interface
            foreach (var obj in Driver.Devices?.SelectMany(d => d.OutputMode?.Elements ?? (IEnumerable<object>)Array.Empty<object>()))
                if (obj is IDisposable disposable)
                    disposable.Dispose();

            Settings = settings ??= Settings.GetDefaults();

            foreach (var dev in Driver.Devices)
            {
                string group = dev.Properties.Name;
                var profile = Settings.Profiles[dev];

                var pluginRef = profile.OutputMode?.GetPluginReference() ?? AppInfo.PluginManager.GetPluginReference(typeof(AbsoluteMode));
                dev.OutputMode = pluginRef.Construct<IOutputMode>();

                if (dev.OutputMode != null)
                    Log.Write(group, $"Output mode: {pluginRef.Name ?? pluginRef.Path}");

                if (dev.OutputMode is AbsoluteOutputMode absoluteMode)
                    SetAbsoluteModeSettings(dev, absoluteMode, profile.AbsoluteModeSettings);

                if (dev.OutputMode is RelativeOutputMode relativeMode)
                    SetRelativeModeSettings(dev, relativeMode, profile.RelativeModeSettings);

                if (dev.OutputMode is IOutputMode outputMode)
                {
                    SetOutputModeSettings(dev, outputMode, profile);
                    SetBindingHandlerSettings(dev, outputMode, profile.BindingSettings);
                }
            }

            Log.Write("Settings", "Driver is enabled.");

            SetToolSettings();

            return Task.CompletedTask;
        }

        public async Task ResetSettings()
        {
            await SetSettings(Settings.GetDefaults());
        }

        private void SetOutputModeSettings(InputDeviceTree dev, IOutputMode outputMode, Profile profile)
        {
            string group = dev.Properties.Name;
            outputMode.Tablet = dev;

            var elements = from store in profile.Filters
                where store.Enable == true
                let filter = store.Construct<IPositionedPipelineElement<IDeviceReport>>()
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

        private void SetBindingHandlerSettings(InputDeviceTree dev, IOutputMode outputMode, BindingSettings settings)
        {
            string group = dev.Properties.Name;
            var bindingHandler = new BindingHandler(outputMode);

            var bindingServiceProvider = new ServiceManager();
            object pointer = outputMode switch
            {
                AbsoluteOutputMode absoluteOutputMode => absoluteOutputMode.Pointer,
                RelativeOutputMode relativeOutputMode => relativeOutputMode.Pointer,
                _ => null
            };

            if (pointer is IVirtualMouse virtualMouse)
                bindingServiceProvider.AddService<IVirtualMouse>(() => virtualMouse);

            var tipbinding = bindingHandler.TipBinding = settings.TipButton?.Construct<IBinding>();
            bindingServiceProvider.Inject(tipbinding);
            bindingHandler.TipActivationPressure = settings.TipActivationPressure;
            Log.Write(group, $"Tip Binding: [{bindingHandler.TipBinding}]@{bindingHandler.TipActivationPressure}%");

            var eraserBinding = bindingHandler.EraserBinding = settings.EraserButton?.Construct<IBinding>();
            bindingServiceProvider.Inject(eraserBinding);
            bindingHandler.EraserActivationPressure = settings.EraserActivationPressure;
            Log.Write(group, $"Eraser Binding: [{bindingHandler.EraserBinding}]@{bindingHandler.EraserActivationPressure}%");

            if (settings.PenButtons != null)
            {
                for (int index = 0; index < settings.PenButtons.Count; index++)
                {
                    var bind = settings.PenButtons[index]?.Construct<IBinding>();
                    if (!bindingHandler.PenButtonBindings.TryAdd(index, bind))
                    {
                        bindingHandler.PenButtonBindings[index] = bind;
                        bindingServiceProvider.Inject(bind);
                    }
                }

                Log.Write(group, $"Pen Bindings: " + string.Join(", ", bindingHandler.PenButtonBindings));
            }

            if (settings.AuxButtons != null)
            {
                for (int index = 0; index < settings.AuxButtons.Count; index++)
                {
                    var bind = settings.AuxButtons[index]?.Construct<IBinding>();
                    if (!bindingHandler.AuxButtonBindings.TryAdd(index, bind))
                    {
                        bindingHandler.AuxButtonBindings[index] = bind;
                        bindingServiceProvider.Inject(bind);
                    }
                }

                Log.Write(group, $"Express Key Bindings: " + string.Join(", ", bindingHandler.AuxButtonBindings));
            }
        }

        private void SetToolSettings()
        {
            foreach (var runningTool in Tools)
                runningTool.Dispose();
            Tools.Clear();

            foreach (PluginSettingStore store in Settings.Tools)
            {
                if (store.Enable == false)
                    continue;

                var tool = store.Construct<ITool>();

                if (tool?.Initialize() ?? false)
                    Tools.Add(tool);
                else
                    Log.Write("Tool", $"Failed to initialize {store.GetPluginReference().Name} tool.", LogLevel.Error);
            }
        }

        public Task<Settings> GetSettings()
        {
            return Task.FromResult(Settings);
        }

        public Task<AppInfo> GetApplicationInfo()
        {
            return Task.FromResult(AppInfo.Current);
        }

        public Task SetTabletDebug(bool enabled)
        {
            debugging = enabled;
            foreach (var dev in Driver.Devices.SelectMany(d => d.InputDevices))
                dev.RawClone = debugging;

            Log.Debug("Tablet", $"Tablet debugging is {(debugging ? "enabled" : "disabled")}");

            return Task.CompletedTask;
        }

        public Task<string> RequestDeviceString(int vid, int pid, int index)
        {
            var tablet = DeviceList.Local.GetHidDevices(vendorID: vid, productID: pid).FirstOrDefault();
            if (tablet == null)
                throw new IOException("Device not found");

            return Task.FromResult(tablet.GetDeviceString(index));
        }

        public Task<IEnumerable<LogMessage>> GetCurrentLog()
        {
            IEnumerable<LogMessage> messages = LogMessages;
            return Task.FromResult(messages);
        }

        private void PostDebugReport(TabletReference tablet, IDeviceReport report)
        {
            if (report != null && tablet != null)
                DeviceReport?.Invoke(this, new DebugReportData(tablet, report));
        }
    }
}
