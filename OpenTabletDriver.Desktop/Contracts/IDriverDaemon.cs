using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Tablet;

#nullable enable

namespace OpenTabletDriver.Desktop.Contracts
{
    public interface IDriverDaemon
    {
        event EventHandler<LogMessage> Message;
        event EventHandler<DebugReportData> DeviceReport;
        event EventHandler<IEnumerable<TabletConfiguration>>? TabletsChanged;

        Task Initialize();

        Task WriteMessage(LogMessage message);

        Task LoadPlugins();
        Task<bool> CheckAssemblyHashes(string remoteHash);
        Task<bool> InstallPlugin(string filePath);
        Task<bool> UninstallPlugin(PluginMetadata metadata);
        Task<bool> DownloadPlugin(PluginMetadata metadata);
        Task<IEnumerable<PluginMetadata>> GetRemotePlugins(string owner = "OpenTabletDriver", string name = "Plugin-Repository", string gitRef = "master");

        Task<IEnumerable<IDeviceEndpoint>> GetDevices();
        Task<IEnumerable<IDisplay>> GetDisplays();

        Task<IEnumerable<TabletConfiguration>> GetTablets();
        Task<IEnumerable<TabletConfiguration>> DetectTablets();

        Task SaveSettings(Settings settings);
        Task ApplySettings(Settings settings);
        Task<Settings> GetSettings();
        Task<Settings> ResetSettings();

        Task ApplyPreset(string name);
        Task<IEnumerable<string>> GetPresets();
        Task SavePreset(string name, Settings settings);

        Task<IAppInfo> GetApplicationInfo();
        Task<IDiagnosticInfo> GetDiagnostics();

        Task SetTabletDebug(bool isEnabled);
        Task<string?> RequestDeviceString(int vendorID, int productID, int index);

        Task<IEnumerable<LogMessage>> GetCurrentLog();

        Task<PluginSettings> GetDefaults(string path);

        Task<TypeProxy> GetProxiedType(string typeName);
        Task<IEnumerable<TypeProxy>> GetMatchingTypes(string typeName);

        Task<bool> HasUpdate();
        Task<UpdateInfo?> GetUpdateInfo();
        Task InstallUpdate();
        Task<IEnumerable<PluginMetadata>> GetInstalledPlugins();
    }
}
