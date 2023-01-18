using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Contracts
{
    public interface IDriverDaemon
    {
        event EventHandler<LogMessage> Message;
        event EventHandler<DebugReportData> DeviceReport;
        event EventHandler<int>? TabletAdded;
        event EventHandler<int>? TabletRemoved;
        event EventHandler<TabletProperty<InputDeviceState>>? TabletStateChanged;
        event EventHandler<TabletProperty<Profile>>? TabletProfileChanged;
        event EventHandler<Settings>? SettingsChanged;
        event EventHandler<PluginEventType>? AssembliesChanged;

        Task WriteMessage(LogMessage message);

        Task<bool> CheckAssemblyHashes(string remoteHash);
        Task<bool> InstallPlugin(string filePath);
        Task<bool> UninstallPlugin(PluginMetadata metadata);
        Task<bool> DownloadPlugin(PluginMetadata metadata);
        Task<IEnumerable<PluginMetadata>> GetRemotePlugins(string owner = "OpenTabletDriver", string name = "Plugin-Repository", string gitRef = "master");
        Task<IEnumerable<PluginMetadata>> GetInstalledPlugins();

        Task<IEnumerable<IDeviceEndpoint>> GetDevices();
        Task<IEnumerable<IDisplay>> GetDisplays();

        Task DetectTablets();
        Task<IEnumerable<int>> GetTablets();
        Task<int> GetPersistentId(int tabletId);
        Task<TabletConfiguration> GetTabletConfiguration(int tabletId);
        Task<InputDeviceState> GetTabletState(int tabletId);
        Task<Profile> GetTabletProfile(int tabletId);
        Task SetTabletProfile(int tabletId, Profile profile);
        Task ResetTabletProfile(int tabletId);

        Task SaveSettings();
        Task ApplySettings(Settings settings);
        Task<Settings> GetSettings();
        Task<Settings> ResetSettings();

        Task<IReadOnlyCollection<string>> GetPresets();
        Task ApplyPreset(string name);
        Task SavePreset(string name, Settings settings);

        Task<IAppInfo> GetApplicationInfo();
        Task<IDiagnosticInfo> GetDiagnostics();

        Task SetTabletDebug(bool isEnabled);
        Task<string?> RequestDeviceString(int vendorID, int productID, int index);

        Task<IEnumerable<LogMessage>> GetCurrentLog();

        Task<PluginSettings> GetDefaults(string path);
        Task<IEnumerable<TypeProxy>> GetMatchingTypes(string typeName);

        Task<SerializedUpdateInfo?> CheckForUpdates();
        Task InstallUpdate();

        Task Initialize();
    }
}
