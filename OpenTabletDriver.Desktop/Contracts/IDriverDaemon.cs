using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Contracts
{
    public interface IDriverDaemon
    {
        event EventHandler<LogMessage> Message;
        event EventHandler<(TabletHandlerID, RpcData)> DebugReport;
        event EventHandler<TabletHandlerID> TabletHandlerCreated;
        event EventHandler<TabletHandlerID> TabletHandlerDestroyed;

        Task WriteMessage(LogMessage message);

        Task LoadPlugins();
        Task<bool> InstallPlugin(string filePath);
        Task<bool> UninstallPlugin(string friendlyName);
        Task<bool> DownloadPlugin(PluginMetadata metadata);

        Task<TabletState> GetTablet(TabletHandlerID id);
        Task<IEnumerable<TabletHandlerID>> GetActiveTabletHandlerIDs();
        Task<IEnumerable<TabletHandlerID>> DetectTablets();

        Task SetSettings(Settings settings);
        Task<Settings> GetSettings();
        Task ResetSettings();

        Task SetProfile(TabletHandlerID id, Profile profile);
        Task<Profile> GetProfile(TabletHandlerID id);
        Task<IEnumerable<Profile>> GetCompatibleProfiles(TabletHandlerID id);
        Task ResetProfile(TabletHandlerID id);

        Task<AppInfo> GetApplicationInfo();

        Task EnableInput(TabletHandlerID id, bool isHooked);

        Task SetTabletDebug(TabletHandlerID id, bool isEnabled);
        Task<string> RequestDeviceString(TabletHandlerID id, int index);
        Task<string> RequestDeviceString(int vendorID, int productID, int index);

        Task<IEnumerable<LogMessage>> GetCurrentLog();
    }
}
