using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Contracts
{
    public interface IDriverDaemon
    {
        event EventHandler<LogMessage> Message;
        event EventHandler<DebugReportData> DeviceReport;
        event EventHandler<IEnumerable<TabletReference>> TabletsChanged;

        Task WriteMessage(LogMessage message);

        Task LoadPlugins();
        Task<bool> InstallPlugin(string filePath);
        Task<bool> UninstallPlugin(string friendlyName);
        Task<bool> DownloadPlugin(PluginMetadata metadata);

        Task<IEnumerable<SerializedDeviceEndpoint>> GetDevices();

        Task<IEnumerable<TabletReference>> GetTablets();
        Task<IEnumerable<TabletReference>> DetectTablets();

        Task SetSettings(Settings settings);
        Task<Settings> GetSettings();
        Task ResetSettings();

        Task<AppInfo> GetApplicationInfo();

        Task SetTabletDebug(bool isEnabled);
        Task<string> RequestDeviceString(int vendorID, int productID, int index);

        Task<IEnumerable<LogMessage>> GetCurrentLog();

        Task<bool> HasUpdate();
        Task<Release> GetUpdateInfo();
        Task InstallUpdate();
    }
}
