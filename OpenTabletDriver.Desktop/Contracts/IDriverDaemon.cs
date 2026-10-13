using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Plugin.Devices;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Contracts
{
    public interface IDriverDaemon
    {
        event EventHandler<LogMessage>? Message;
        event EventHandler<DebugReportData>? DeviceReport;
        event EventHandler<IEnumerable<TabletReference>>? TabletsChanged;
        event EventHandler? Resynchronize;

        Task WriteMessage(LogMessage message);

        Task LoadPlugins();
        Task<bool> InstallPlugin(string filePath);
        Task<bool> UninstallPlugin(string friendlyName);
        Task<bool> DownloadPlugin(PluginMetadata metadata);

        Task<IEnumerable<SerializedDeviceEndpoint>> GetDevices();

        Task<IEnumerable<TabletReference>> GetTablets();
        Task<IEnumerable<TabletReference>> DetectTablets();

        /// <summary>
        /// Serialize settings to configuration file without applying them
        /// </summary>
        /// <param name="settings">(optional) Settings to save, if null, use last settings set with <see cref="SetSettings"/></param>
        /// <param name="filePath">(optional) Path to save settings to. Will save to the default configuration file if not specified.</param>
        Task SaveSettings(Settings? settings = null, string? filePath = null);
        Task SetSettings(Settings settings);
        Task<Settings> GetSettings();
        Task ResetSettings();

        Task<AppInfo> GetApplicationInfo();

        Task SetTabletDebug(bool isEnabled);
        Task<string> RequestDeviceString(int vendorID, int productID, int index);
        Task SendFeatureInit(int vendorID, int productID, string strInterface, byte[] initData);
        Task SendOutputInit(int vendorID, int productID, string strInterface, byte[] initData);

        Task<IEnumerable<LogMessage>> GetCurrentLog();
        Task<DiagnosticInfo> GetDiagnosticInfo();

        Task<SerializedUpdateInfo?> CheckForUpdates();
        Task InstallUpdate();

        Task ForceResynchronize();
    }
}
