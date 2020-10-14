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

        Task<bool> SetTablet(TabletConfiguration tablet);
        Task<TabletStatus> GetTablet();
        
        Task<TabletStatus> DetectTablets();

        Task SetSettings(Settings settings);
        Task<Settings> GetSettings();

        Task<AppInfo> GetApplicationInfo();
        
        Task<bool> LoadPlugins();
        Task<bool> ImportPlugin(string pluginPath);

        Task EnableInput(bool isHooked);
        
        Task SetTabletDebug(bool isEnabled);
        Task<string> RequestDeviceString(int index);

        Task<IEnumerable<LogMessage>> GetCurrentLog();
    }
}