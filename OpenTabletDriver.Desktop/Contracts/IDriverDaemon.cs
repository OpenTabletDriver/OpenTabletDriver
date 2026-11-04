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
using PolyType;
using StreamJsonRpc;

namespace OpenTabletDriver.Desktop.Contracts
{
    [GenerateShapeFor<LogMessage>, GenerateShapeFor<DebugReportData>, GenerateShapeFor<TabletReference>]
    [GenerateShapeFor<bool>, GenerateShapeFor<SerializedDeviceEndpoint>, GenerateShapeFor<Settings>]
    [GenerateShapeFor<AppInfo>, GenerateShapeFor<string>, GenerateShapeFor<DiagnosticInfo>, GenerateShapeFor<SerializedUpdateInfo>]
    partial class Witness;

    [JsonRpcContract]
    [GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
    [RpcMarshalable(CallScopedLifetime = true)]
    public partial interface IDriverDaemon
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
