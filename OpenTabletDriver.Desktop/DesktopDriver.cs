using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop
{
    public class DesktopDriver : Driver, IVirtualDisplayDriver
    {
        public DesktopDriver() : base()
        {
            ReportHandled += (_, report) => BindingHandler.HandleBinding(Tablet, report);
        }

        protected override PluginManager PluginManager => AppInfo.PluginManager;
        public IVirtualScreen VirtualScreen => SystemInterop.VirtualScreen;
    }
}
