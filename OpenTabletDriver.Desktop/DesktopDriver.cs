using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopDriver : Driver, IVirtualDisplayDriver
    {
        public override void HandleReport(IDeviceReport report)
        {
            base.HandleReport(report);
            BindingHandler.HandleBinding(Tablet, report);
        }

        protected override IReportParser<IDeviceReport> GetReportParser(DeviceIdentifier identifier)
        {
            var pluginRef = AppInfo.PluginManager.GetPluginReference(identifier.ReportParser);
            return pluginRef.Construct<IReportParser<IDeviceReport>>();
        }

        IVirtualScreen IVirtualDisplayDriver.VirtualScreen => SystemInterop.VirtualScreen;
    }
}
