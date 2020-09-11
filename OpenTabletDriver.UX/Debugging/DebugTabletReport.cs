using System.Numerics;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.UX.Debugging
{
    public class DebugTabletReport : DebugDeviceReport, ITabletReport
    {
        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}