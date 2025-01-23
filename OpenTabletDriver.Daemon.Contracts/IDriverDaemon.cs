using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.Logging;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Contracts
{
    public interface IDriverDaemon
    {
        event EventHandler<LogMessage> Message;
        event EventHandler<int>? TabletAdded;
        event EventHandler<int>? TabletRemoved;
        event EventHandler<TabletProperty<InputDeviceState>>? TabletStateChanged;
        event EventHandler<TabletProperty<Profile>>? TabletProfileChanged;
        event EventHandler<IEnumerable<PluginSettings>>? ToolsChanged;
        event EventHandler<PluginContextDto>? PluginAdded;
        event EventHandler<PluginContextDto>? PluginRemoved;

        Task Initialize();

        Task<IEnumerable<PluginMetadata>> GetInstallablePlugins(string owner = "OpenTabletDriver", string name = "Plugin-Repository", string gitRef = "master");
        Task<IEnumerable<PluginContextDto>> GetPlugins();
        Task<bool> InstallPluginFromLocal(string filePath);
        Task<bool> InstallPluginFromRemote(PluginMetadata metadata);
        Task<bool> UninstallPlugin(string pluginName);

        Task<IEnumerable<DeviceEndpointDto>> GetDevices();
        Task<IEnumerable<DisplayDto>> GetDisplays();

        Task DetectTablets();
        Task<IEnumerable<int>> GetTablets();
        Task<int> GetTabletPersistentId(int tabletId);
        Task<InputDeviceState> GetTabletState(int tabletId);
        Task SetTabletState(int tabletId, InputDeviceState state);
        Task<TabletConfiguration> GetTabletConfiguration(int tabletId);
        Task<Profile> GetTabletProfile(int tabletId);
        Task SetTabletProfile(int tabletId, Profile profile);
        Task ResetTabletProfile(int tabletId);

        Task<IEnumerable<PluginSettings>> GetToolSettings();
        Task SetToolSettings(IEnumerable<PluginSettings> toolSettings);
        Task ResetToolSettings();

        Task SaveSettings();
        Task ResetSettings();

        Task<IEnumerable<string>> GetPresets();
        Task ApplyPreset(string name);
        Task SaveAsPreset(string name);

        Task<AppInfo> GetApplicationInfo();
        Task<DiagnosticInfo> GetDiagnostics();

        Task SetTabletDebug(bool isEnabled);
        Task<string?> RequestDeviceString(int tabletId, int index);
        Task<string?> RequestDeviceString(int vendorID, int productID, int index);

        Task WriteMessage(LogMessage message);
        Task<IEnumerable<LogMessage>> GetCurrentLog();

        Task<UpdateInfoDto?> CheckForUpdates();
        Task InstallUpdate();
    }
}
