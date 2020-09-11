using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Contracts
{
    public interface IDriverDaemon
    {
        bool SetTablet(TabletProperties tablet);
        TabletProperties GetTablet();
        
        TabletProperties DetectTablets();

        void SetSettings(Settings settings);
        Settings GetSettings();

        AppInfo GetApplicationInfo();
        
        bool LoadPlugins();
        bool ImportPlugin(string pluginPath);

        void SetInputHook(bool isHooked);
        IEnumerable<Guid> SetTabletDebug(bool isEnabled);
        
        Guid SetLogOutput(bool isEnabled);
        IEnumerable<LogMessage> GetCurrentLog();
    }
}