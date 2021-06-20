using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME), SupportedPlatform(PluginPlatform.Linux)]
    public class PenPassthroughBinding : IInterruptBinding
    {
        private const string PLUGIN_NAME = "Pen Passthrough";

        [Resolved]
        public IVirtualTablet Tablet { set; get; }

        public void Invoke(TabletReference tablet, IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                float pressure = (float)tabletReport.Pressure / (float)tablet.Properties.Specifications.Pen.MaxPressure;
                bool isEraser = report is IEraserReport eraserReport ? eraserReport.Eraser : false;
                Tablet.SetPressure(pressure, isEraser);
            }
        }
    }
}