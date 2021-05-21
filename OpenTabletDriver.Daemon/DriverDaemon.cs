using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using HidSharp;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Migration;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Output;
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
            Driver.TabletChanged += (sender, tablet) =>
            {
                TabletChanged?.Invoke(sender, tablet);
                if (debugging)
                {
                    if (Driver.TabletReader != null)
                    {
                        Driver.TabletReader.RawClone = true;
                        Driver.TabletReader.RawReport += DebugReportHandler;
                    }
                    if (Driver.AuxReader != null)
                    {
                        Driver.AuxReader.RawClone = true;
                        Driver.AuxReader.RawReport += DebugReportHandler;
                    }
                }
            };
            Driver.DevicesChanged += async (sender, args) =>
            {
                if (await GetTablet() == null && args.Additions.Count() > 0)
                    await DetectTablets();
            };

            DesktopInterop.PowerManager.PowerEvent += async (object sender, PowerEventArgs e) =>
            {
                if (e.EventType == PowerEventType.Resume)
                {
                    await DetectTablets();
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
                var settings = Settings.Deserialize(settingsFile);
                if (settings != null)
                {
                    await SetSettings(settings);
                }
                else
                {
                    Log.Write("Settings", "Invalid settings detected. Attempting recovery.", LogLevel.Error);
                    settings = Settings.Default;
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
        public event EventHandler<RpcData> DeviceReport;
        public event EventHandler<TabletState> TabletChanged;

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
            if (pluginDir.Exists)
            {
                var pluginManager = AppInfo.PluginManager;
                pluginManager.Load();

                // Migrate if settings is available to avoid invalid settings
                if (Settings != null)
                    Settings = SettingsMigrator.Migrate(Settings);

                // Add services to inject on plugin construction
                pluginManager.AddService<IDriver>(() => this.Driver);
            }
            else
            {
                pluginDir.Create();
                Log.Write("Plugin", $"The plugin directory '{pluginDir.FullName}' has been created");
            }
            return Task.CompletedTask;
        }

        public Task<bool> InstallPlugin(string filePath)
        {
            return Task.FromResult(AppInfo.PluginManager.InstallPlugin(filePath));
        }

        public Task<bool> UninstallPlugin(string friendlyName)
        {
            var plugins = AppInfo.PluginManager.GetLoadedPlugins();
            var plugin = plugins.FirstOrDefault(ctx => ctx.FriendlyName == friendlyName);
            return Task.FromResult(AppInfo.PluginManager.UninstallPlugin(plugin));
        }

        public Task<bool> DownloadPlugin(PluginMetadata metadata)
        {
            return AppInfo.PluginManager.DownloadPlugin(metadata);
        }

        public Task<TabletState> GetTablet()
        {
            return Task.FromResult(Driver.Tablet);
        }

        public async Task<TabletState> DetectTablets()
        {
            var configDir = new DirectoryInfo(AppInfo.Current.ConfigurationDirectory);
            if (configDir.Exists)
            {
                foreach (var file in configDir.EnumerateFiles("*.json", SearchOption.AllDirectories))
                {
                    var tablet = Serialization.Deserialize<TabletConfiguration>(file);
                    if (Driver.TryMatch(tablet))
                        return await GetTablet();
                }
            }
            else
            {
                Log.Write("Detect", $"The configuration directory '{configDir.FullName}' does not exist.", LogLevel.Error);
            }
            Log.Write("Detect", "No tablet found.");
            return null;
        }

        public async Task SetSettings(Settings settings)
        {
            // Dispose filters that implement IDisposable interface
            if (Driver.OutputMode?.Elements != null)
            {
                foreach (var element in Driver.OutputMode.Elements)
                {
                    try
                    {
                        if (element is IDisposable disposable)
                            disposable.Dispose();
                    }
                    catch (Exception)
                    {
                        Log.Write("Plugin", $"Unable to dispose object '{element.GetType().Name}'", LogLevel.Error);
                    }
                }
            }

            if (settings == null)
                await ResetSettings();

            Settings = SettingsMigrator.Migrate(settings);

            var pluginRef = Settings.OutputMode?.GetPluginReference() ?? AppInfo.PluginManager.GetPluginReference(typeof(AbsoluteMode));
            Driver.OutputMode = pluginRef.Construct<IOutputMode>();

            if (Driver.OutputMode != null)
                Log.Write("Settings", $"Output mode: {pluginRef.Name ?? pluginRef.Path}");

            if (Driver.OutputMode is AbsoluteOutputMode absoluteMode)
                SetAbsoluteModeSettings(absoluteMode);

            if (Driver.OutputMode is RelativeOutputMode relativeMode)
                SetRelativeModeSettings(relativeMode);

            if (Driver.OutputMode is IOutputMode outputMode)
                SetOutputModeSettings(outputMode);

            SetBindingHandlerSettings();

            if (Settings.AutoHook)
            {
                Driver.EnableInput = true;
                Log.Write("Settings", "Driver is auto-enabled.");
            }

            SetToolSettings();
        }

        public async Task ResetSettings()
        {
            await SetSettings(Settings.Default);
        }

        private void SetOutputModeSettings(IOutputMode outputMode)
        {
            outputMode.Tablet = Driver.Tablet;

            var elements = from store in Settings.Filters
                where store.Enable == true
                let filter = store.Construct<IPositionedPipelineElement<IDeviceReport>>()
                where filter != null
                select filter;
            outputMode.Elements = elements.ToList();

            if (outputMode.Elements != null && outputMode.Elements.Count() > 0)
                Log.Write("Settings", $"Filters: {string.Join(", ", outputMode.Elements)}");
        }

        private void SetAbsoluteModeSettings(AbsoluteOutputMode absoluteMode)
        {
            absoluteMode.Output = new Area
            {
                Width = Settings.DisplayWidth,
                Height = Settings.DisplayHeight,
                Position = new Vector2
                {
                    X = Settings.DisplayX,
                    Y = Settings.DisplayY
                }
            };
            Log.Write("Settings", $"Display area: {absoluteMode.Output}");

            absoluteMode.Input = new Area
            {
                Width = Settings.TabletWidth,
                Height = Settings.TabletHeight,
                Position = new Vector2
                {
                    X = Settings.TabletX,
                    Y = Settings.TabletY
                },
                Rotation = Settings.TabletRotation
            };
            Log.Write("Settings", $"Tablet area: {absoluteMode.Input}");

            absoluteMode.AreaClipping = Settings.EnableClipping;
            Log.Write("Settings", $"Clipping: {(absoluteMode.AreaClipping ? "Enabled" : "Disabled")}");

            absoluteMode.AreaLimiting = Settings.EnableAreaLimiting;
            Log.Write("Settings", $"Ignoring reports outside area: {(absoluteMode.AreaLimiting ? "Enabled" : "Disabled")}");
        }

        private void SetRelativeModeSettings(RelativeOutputMode relativeMode)
        {
            relativeMode.Sensitivity = new Vector2(Settings.XSensitivity, Settings.YSensitivity);
            Log.Write("Settings", $"Relative Mode Sensitivity (X, Y): {relativeMode.Sensitivity}");

            relativeMode.Rotation = Settings.RelativeRotation;
            Log.Write("Settings", $"Relative Mode Rotation: {relativeMode.Rotation}");

            relativeMode.ResetTime = Settings.ResetTime;
            Log.Write("Settings", $"Reset time: {relativeMode.ResetTime}");
        }

        private void SetBindingHandlerSettings()
        {
            BindingHandler.TipBinding = Settings.TipButton?.Construct<IBinding>();
            BindingHandler.TipActivationPressure = Settings.TipActivationPressure;
            Log.Write("Settings", $"Tip Binding: [{BindingHandler.TipBinding}]@{BindingHandler.TipActivationPressure}%");

            BindingHandler.EraserBinding = Settings.EraserButton?.Construct<IBinding>();
            BindingHandler.EraserActivationPressure = Settings.EraserActivationPressure;
            Log.Write("Settings", $"Eraser Binding: [{BindingHandler.EraserBinding}]@{BindingHandler.EraserActivationPressure}%");

            if (Settings.PenButtons != null)
            {
                for (int index = 0; index < Settings.PenButtons.Count; index++)
                {
                    var bind = Settings.PenButtons[index]?.Construct<IBinding>();
                    if (!BindingHandler.PenButtonBindings.TryAdd(index, bind))
                        BindingHandler.PenButtonBindings[index] = bind;
                }

                Log.Write("Settings", $"Pen Bindings: " + string.Join(", ", BindingHandler.PenButtonBindings));
            }

            if (Settings.AuxButtons != null)
            {
                for (int index = 0; index < Settings.AuxButtons.Count; index++)
                {
                    var bind = Settings.AuxButtons[index]?.Construct<IBinding>();
                    if (!BindingHandler.AuxButtonBindings.TryAdd(index, bind))
                        BindingHandler.AuxButtonBindings[index] = bind;
                }

                Log.Write("Settings", $"Express Key Bindings: " + string.Join(", ", BindingHandler.AuxButtonBindings));
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

        public Task EnableInput(bool isHooked)
        {
            Driver.EnableInput = isHooked;
            return Task.CompletedTask;
        }

        public Task SetTabletDebug(bool enabled)
        {
            if (Driver.TabletReader != null)
                Driver.TabletReader.RawClone = enabled;
            if (Driver.AuxReader != null)
                Driver.AuxReader.RawClone = enabled;

            if (enabled && !debugging)
            {
                if (Driver.TabletReader != null)
                    Driver.TabletReader.RawReport += DebugReportHandler;
                if (Driver.AuxReader != null)
                    Driver.AuxReader.RawReport += DebugReportHandler;
                debugging = true;
            }
            else if (!enabled && debugging)
            {
                if (Driver.TabletReader != null)
                    Driver.TabletReader.RawReport -= DebugReportHandler;
                if (Driver.AuxReader != null)
                    Driver.AuxReader.RawReport -= DebugReportHandler;
                debugging = false;
            }

            return Task.CompletedTask;
        }

        public Task<string> RequestDeviceString(int index)
        {
            if (Driver.TabletReader?.Device != null)
                return Task.FromResult(Driver.TabletReader.Device.GetDeviceString(index));
            else
                throw new IOException("Device not found");
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

        private void DebugReportHandler(object _, IDeviceReport report)
        {
            if (report != null)
                DeviceReport?.Invoke(this, new RpcData(report));
        }
    }
}
