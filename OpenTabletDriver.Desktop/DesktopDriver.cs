using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop
{
    public class DesktopDriver : Driver, IVirtualDisplayDriver
    {
        protected override PluginManager PluginManager => AppInfo.PluginManager;

        public override void HandleReport(IDeviceReport report)
        {
            base.HandleReport(report);
            BindingHandler.HandleBinding(Tablet, report);
        }

        public IVirtualScreen VirtualScreen => SystemInterop.VirtualScreen;
    }
}
