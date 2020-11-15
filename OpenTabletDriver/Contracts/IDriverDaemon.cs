using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTabletDriver.Debugging;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Contracts
{
    public interface IDriverDaemon
    {
        event EventHandler<LogMessage> Message;
        event EventHandler<DebugTabletReport> TabletReport;
        event EventHandler<DebugAuxReport> AuxReport;
        event EventHandler<TabletStatus> TabletChanged;

        Task WriteMessage(LogMessage message);

        Task LoadPlugins();

        Task<TabletStatus> GetTablet();
        Task<TabletStatus> DetectTablets();

        Task SetSettings(Settings settings);
        Task<Settings> GetSettings();

        Task<AppInfo> GetApplicationInfo();

        Task EnableInput(bool isHooked);

        Task SetTabletDebug(bool isEnabled);
        Task<string> RequestDeviceString(int index);
        Task<string> RequestDeviceString(int vendorID, int productID, int index);

        Task<IEnumerable<LogMessage>> GetCurrentLog();
    }
}