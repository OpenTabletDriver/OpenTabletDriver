using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop
{
    public class DesktopDriver : Driver
    {
        protected override PluginManager PluginManager => AppInfo.PluginManager;

        public override void HandleReport(IDeviceReport report)
        {
            base.HandleReport(report);
            BindingHandler.HandleBinding(Tablet, report);
        }
    }
}
