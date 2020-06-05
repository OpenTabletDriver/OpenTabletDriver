using System.Collections.Generic;
using System.Threading.Tasks;
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
        
        Task<bool> LoadPlugins();
        Task<bool> ImportPlugin(string pluginPath);

        void SetInputHook(bool isHooked);
        void SetTabletDebug(bool isEnabled);

        IEnumerable<string> GetChildTypes<T>();
    }
}