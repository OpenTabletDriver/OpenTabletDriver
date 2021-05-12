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

            Driver.TabletHandlerCreated += (sender, tabletHandlerID) =>
            {
                SetProfile(tabletHandlerID, Settings.DeserializeActiveProfile(tabletHandlerID));
                TabletHandlerCreated?.Invoke(this, tabletHandlerID);
            };

            Driver.TabletHandlerDestroyed += (sender, tabletHandlerID) =>
            {
                TabletHandlerDestroyed?.Invoke(this, tabletHandlerID);
            };

            Driver.DevicesChanged += (sender, args) =>
            {
                if (args.Additions.Any())
                    Driver.ProcessDevices(args.Additions, tabletConfigurations);
            };

            InitializeAsync();
        }

        public DesktopDriver Driver { get; } = new DesktopDriver();

        public event EventHandler<LogMessage> Message;
        public event EventHandler<(TabletHandlerID, RpcData)> DebugReport;
        public event EventHandler<TabletHandlerID> TabletHandlerCreated;
        public event EventHandler<TabletHandlerID> TabletHandlerDestroyed;

        private Collection<LogMessage> LogMessages = new Collection<LogMessage>();
        private Collection<ITool> Tools = new Collection<ITool>();
        private Settings Settings;
        private Dictionary<TabletHandlerID, Profile> Profiles = new Dictionary<TabletHandlerID, Profile>();
        private TabletConfiguration[] tabletConfigurations;

        private async void InitializeAsync()
        {
            AppInfo.PluginManager.Clean();
            await LoadPlugins();

            var appdataDir = new DirectoryInfo(AppInfo.Current.AppDataDirectory);
            if (!appdataDir.Exists)
            {
                appdataDir.Create();
                Log.Write("Settings", $"Created OpenTabletDriver application data directory: {appdataDir.FullName}");
            }

            var settingsFile = new FileInfo(AppInfo.Current.SettingsFile);

            if (settingsFile.Exists)
            {
                var settings = Serialization.Deserialize<Settings>(settingsFile);
                if (settings != null)
                    await SetSettings(settings);
            }

            if (Settings == null)
                await ResetSettings();

            await DetectTablets();
        }

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

                // Migrate if profiles are available to avoid invalid settings
                foreach (var tabletHandle in Driver.GetActiveTabletHandlerIDs())
                {
                    Profiles[tabletHandle] = ProfileMigrator.Migrate(Profiles[tabletHandle]);
                }

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

        public Task<TabletState> GetTablet(TabletHandlerID id)
        {
            return Task.FromResult(Driver.GetTabletState(id));
        }

        public Task<IEnumerable<TabletHandlerID>> GetActiveTabletHandlerIDs()
        {
            return Task.FromResult(Driver.GetActiveTabletHandlerIDs());
        }

        public Task<IEnumerable<TabletHandlerID>> DetectTablets()
        {
            var configDir = new DirectoryInfo(AppInfo.Current.ConfigurationDirectory);
            if (configDir.Exists)
            {
                tabletConfigurations = configDir.EnumerateFiles("*.json", SearchOption.AllDirectories)
                    .Select(f => TryDeserializeTabletConfig(f)).ToArray();
            }
            else
            {
                Log.Write("Detect", $"The configuration directory '{configDir.FullName}' does not exist.", LogLevel.Error);
            }

            Driver.EnumerateTablets(tabletConfigurations);
            return Task.FromResult(Driver.GetActiveTabletHandlerIDs());
        }

        public Task SetSettings(Settings settings)
        {
            Settings = settings;
            SetToolSettings();
            return Task.CompletedTask;
        }

        public Task<Settings> GetSettings()
        {
            return Task.FromResult(Settings);
        }

        public Task ResetSettings()
        {
            Settings = new Settings();
            SetSettings(Settings);
            return Task.CompletedTask;
        }

        public Task SetProfile(TabletHandlerID id, Profile profile)
        {
            var handler = Driver.GetTabletHandler(id);

            Log.Write("Settings", $"Applying profile '{profile.ProfileName}' to {id}: {handler.TabletState.Properties.Name}");
            // Dispose filters that implement IDisposable interface
            if (handler.OutputMode?.Elements != null)
            {
                foreach (var element in handler.OutputMode.Elements)
                {
                    try
                    {
                        if (element is IDisposable disposable)
                            disposable.Dispose();
                    }
                    catch (Exception)
                    {
                        Log.Write("Plugin", $"[{id.Value}]: Unable to dispose object '{element.GetType().Name}'", LogLevel.Error);
                    }
                }
            }

            if (!Profiles.ContainsKey(id))
                Profiles.Add(id, profile);

            profile = ProfileMigrator.Migrate(profile);
            Profiles[id] = profile;

            var pluginRef = profile.OutputMode?.GetPluginReference() ?? AppInfo.PluginManager.GetPluginReference(typeof(AbsoluteMode));
            handler.OutputMode = AppInfo.PluginManager.Tag(handler, pluginRef.Construct<IOutputMode>());

            if (handler.OutputMode != null)
            {
                Log.Write("Settings", $"[{id.Value}]: Output mode: {pluginRef.Name ?? pluginRef.Path}");
                SetOutputModeSettings(handler, profile, handler.OutputMode);
            }

            SetBindingHandlerSettings(handler, profile);

            if (profile.AutoHook)
            {
                handler.EnableInput = true;
                Log.Write("Settings", $"[{id.Value}]: Driver is auto-enabled.");
            }

            Log.Write("Settings", "Done applying profile");

            return Task.CompletedTask;
        }

        public Task<Profile> GetProfile(TabletHandlerID id)
        {
            return Task.FromResult(Profiles.TryGetValue(id, out var profile) ? profile : null);
        }

        public Task<IEnumerable<Profile>> GetCompatibleProfiles(TabletHandlerID id)
        {
            return Task.FromResult(ProfileSerializer.GetCompatibleProfiles(id));
        }

        public Task ResetProfile(TabletHandlerID id)
        {
            Profiles[id] = ProfileSerializer.GetDefaultProfile(id);
            return Task.CompletedTask;
        }

        public Task<AppInfo> GetApplicationInfo()
        {
            return Task.FromResult(AppInfo.Current);
        }

        public Task EnableInput(TabletHandlerID id, bool isHooked)
        {
            Driver.GetTabletHandler(id).EnableInput = isHooked;
            return Task.CompletedTask;
        }

        public Task SetTabletDebug(TabletHandlerID id, bool isEnabled)
        {
            var tabletReader = Driver.GetTabletHandler(id)?.DigitizerReader;
            var auxReader = Driver.GetTabletHandler(id)?.AuxilaryReader;

            if (tabletReader != null)
                tabletReader.RawClone = isEnabled;
            if (auxReader != null)
                auxReader.RawClone = isEnabled;

            if (isEnabled)
            {
                if (tabletReader != null)
                    tabletReader.RawReport += HandleDebugReport;
                if (auxReader != null)
                    auxReader.RawReport += HandleDebugReport;
            }
            else if (!isEnabled)
            {
                if (tabletReader != null)
                    tabletReader.RawReport -= HandleDebugReport;
                if (auxReader != null)
                    auxReader.RawReport -= HandleDebugReport;
            }

            return Task.CompletedTask;
        }

        public Task<string> RequestDeviceString(TabletHandlerID id, int index)
        {
            var deviceReader = Driver.GetTabletHandler(id)?.DigitizerReader;
            if (deviceReader?.Device != null)
                return Task.FromResult(deviceReader.Device.GetDeviceString(index));
            else
                throw new IOException("Device not found");
        }

        public Task<string> RequestDeviceString(int vendorID, int productID, int index)
        {
            var tablet = DeviceList.Local.GetHidDevices(vendorID: vendorID, productID: productID).FirstOrDefault();
            if (tablet == null)
                throw new IOException("Device not found");

            return Task.FromResult(tablet.GetDeviceString(index));
        }

        public Task<IEnumerable<LogMessage>> GetCurrentLog()
        {
            IEnumerable<LogMessage> messages = LogMessages;
            return Task.FromResult(messages);
        }

        private static void SetOutputModeSettings(TabletHandler handler, Profile profile, IOutputMode outputMode)
        {
            if (outputMode is AbsoluteOutputMode absoluteMode)
                SetAbsoluteModeSettings(handler.InstanceID, absoluteMode, profile);
            if (outputMode is RelativeOutputMode relativeMode)
                SetRelativeModeSettings(handler.InstanceID, relativeMode, profile);

            outputMode.Tablet = handler.TabletState;

            var elements = from store in profile.Filters
                           where store.Enable == true
                           let filter = store.Construct<IPositionedPipelineElement<IDeviceReport>>()
                           where filter != null
                           select AppInfo.PluginManager.Tag(handler, filter);
            outputMode.Elements = elements.ToList();

            if (outputMode.Elements != null && outputMode.Elements.Count > 0)
                Log.Write("Settings", $"[{handler.InstanceID.Value}]: Filters: {string.Join(", ", outputMode.Elements)}");
        }

        private static void SetAbsoluteModeSettings(TabletHandlerID id, AbsoluteOutputMode absoluteMode, Profile profile)
        {
            absoluteMode.Output = new Area
            {
                Width = profile.DisplayWidth,
                Height = profile.DisplayHeight,
                Position = new Vector2
                {
                    X = profile.DisplayX,
                    Y = profile.DisplayY
                }
            };
            Log.Write("Settings", $"[{id.Value}]: Display area: {absoluteMode.Output}");

            absoluteMode.Input = new Area
            {
                Width = profile.TabletWidth,
                Height = profile.TabletHeight,
                Position = new Vector2
                {
                    X = profile.TabletX,
                    Y = profile.TabletY
                },
                Rotation = profile.TabletRotation
            };
            Log.Write("Settings", $"[{id.Value}]: Tablet area: {absoluteMode.Input}");

            absoluteMode.AreaClipping = profile.EnableClipping;
            Log.Write("Settings", $"[{id.Value}]: Clipping: {(absoluteMode.AreaClipping ? "Enabled" : "Disabled")}");

            absoluteMode.AreaLimiting = profile.EnableAreaLimiting;
            Log.Write("Settings", $"[{id.Value}]: Ignoring reports outside area: {(absoluteMode.AreaLimiting ? "Enabled" : "Disabled")}");
        }

        private static void SetRelativeModeSettings(TabletHandlerID id, RelativeOutputMode relativeMode, Profile profile)
        {
            relativeMode.Sensitivity = new Vector2(profile.XSensitivity, profile.YSensitivity);
            Log.Write("Settings", $"[{id.Value}]: Relative Mode Sensitivity (X, Y): {relativeMode.Sensitivity}");

            relativeMode.Rotation = profile.RelativeRotation;
            Log.Write("Settings", $"[{id.Value}]: Relative Mode Rotation: {relativeMode.Rotation}");

            relativeMode.ResetTime = profile.ResetTime;
            Log.Write("Settings", $"[{id.Value}]: Reset time: {relativeMode.ResetTime}");
        }

        private static void SetBindingHandlerSettings(TabletHandler handler, Profile profile)
        {
            var bindingHandler = new BindingHandler();
            var id = handler.InstanceID;

            handler.HandleReport = (outputMode, report) =>
            {
                outputMode.Read(report);
                bindingHandler.HandleBinding(handler.TabletState, report);
            };

            bindingHandler.TipBinding = AppInfo.PluginManager.Tag(handler, profile.TipButton?.Construct<IBinding>());
            bindingHandler.TipActivationPressure = profile.TipActivationPressure;
            Log.Write("Settings", $"[{id.Value}]: Tip Binding: [{bindingHandler.TipBinding}]@{bindingHandler.TipActivationPressure}%");

            bindingHandler.EraserBinding = AppInfo.PluginManager.Tag(handler, profile.EraserButton?.Construct<IBinding>());
            bindingHandler.EraserActivationPressure = profile.EraserActivationPressure;
            Log.Write("Settings", $"[{id.Value}]: Eraser Binding: [{bindingHandler.EraserBinding}]@{bindingHandler.EraserActivationPressure}%");

            if (profile.PenButtons != null)
            {
                for (int index = 0; index < profile.PenButtons.Count; index++)
                {
                    var bind = AppInfo.PluginManager.Tag(handler, profile.PenButtons[index]?.Construct<IBinding>());
                    if (!bindingHandler.PenButtonBindings.TryAdd(index, bind))
                        bindingHandler.PenButtonBindings[index] = bind;
                }

                Log.Write("Settings", $"[{id.Value}]: Pen Bindings: " + string.Join(", ", bindingHandler.PenButtonBindings));
            }

            if (profile.AuxButtons != null)
            {
                for (int index = 0; index < profile.AuxButtons.Count; index++)
                {
                    var bind = AppInfo.PluginManager.Tag(handler, profile.AuxButtons[index]?.Construct<IBinding>());
                    if (!bindingHandler.AuxButtonBindings.TryAdd(index, bind))
                        bindingHandler.AuxButtonBindings[index] = bind;
                }

                Log.Write("Settings", $"[{id.Value}]: Express Key Bindings: " + string.Join(", ", bindingHandler.AuxButtonBindings));
            }
        }

        private void SetToolSettings()
        {
            foreach (var tool in Tools)
                tool.Dispose();
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

        private void HandleDebugReport(object _, (TabletHandlerID, IDeviceReport) taggedReport)
        {
            (var id, var report) = taggedReport;
            if (report != null)
                DebugReport?.Invoke(this, (id, new RpcData(report)));
        }

        private static TabletConfiguration TryDeserializeTabletConfig(FileInfo file)
        {
            try
            {
                return Serialization.Deserialize<TabletConfiguration>(file);
            }
            catch (Exception e)
            {
                Log.Write("Detect", $"Failed to deserialize configuration '{file.FullName}'", LogLevel.Error);
                Log.Exception(e);
                return null;
            }
        }
    }
}