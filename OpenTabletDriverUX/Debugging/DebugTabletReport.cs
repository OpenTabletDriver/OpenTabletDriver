using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriverUX.Debugging
{
    public class DebugTabletReport : DebugDeviceReport, ITabletReport
    {
        public uint ReportID { set; get; }
        public Point Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}