using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HidSharp;
using TabletDriverLib.Plugins;
using TabletDriverPlugin;
using TabletDriverPlugin.Logging;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Contracts
{
    public interface IDriverDaemon
    {
        bool SetTablet(TabletProperties tablet);
        TabletProperties GetTablet();

        void SetSettings(Settings settings);
        Settings GetSettings();
                
        void SetOutputMode(PluginReference outputMode);
        IOutputMode GetOutputMode();
        
        Task<bool> ImportPlugin(FileInfo plugin);

        void SetInputHook(bool isHooked);
    }
}