using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Debugging
{
    public class DebugTouchReport : DebugDeviceReport, ITouchReport
    {
        public DebugTouchReport()
        {
        }

        public DebugTouchReport(ITouchReport tabletReport) : base(tabletReport)
        {
            this.Touches = tabletReport.Touches;
        }

        public TouchPoint?[] Touches { set; get; }
    }
}
