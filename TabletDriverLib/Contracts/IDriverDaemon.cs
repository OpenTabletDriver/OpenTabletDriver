using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TabletDriverPlugin.Logging;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Contracts
{
    public interface IDriverDaemon
    {
        bool SetTablet(TabletProperties tablet);
        TabletProperties GetTablet();
        
        TabletProperties DetectTablets();

        void SetSettings(Settings settings);
        Settings GetSettings();

        AppInfo GetApplicationInfo();
        
        Task<bool> LoadPlugins();
        Task<bool> ImportPlugin(string pluginPath);

        void SetInputHook(bool isHooked);
        IEnumerable<Guid> SetTabletDebug(bool isEnabled);
        
        Guid SetLogOutput(bool isEnabled);
        IEnumerable<LogMessage> GetCurrentLog();

        IEnumerable<string> GetChildTypes<T>();
    }
}