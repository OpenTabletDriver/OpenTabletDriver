using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME), SupportedPlatform(PluginPlatform.Linux)]
    public class PenPassthroughBinding : IInterruptBinding
    {
        public PenPassthroughBinding(IVirtualTablet tablet)
        {
            Tablet = tablet;
        }

        private const string PLUGIN_NAME = "Pen Passthrough";

        public IVirtualTablet Tablet { set; get; }

        public void Invoke(TabletReference tablet, IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                float pressure = (float)tabletReport.Pressure / (float)tablet.Properties.Specifications.Pen.MaxPressure;
                Tablet.SetPressure(pressure);
                Tablet.SetButtonState(0, tabletReport.PenButtons[0]);
                Tablet.SetButtonState(1, tabletReport.PenButtons[1]);
            }
        }
    }
}
