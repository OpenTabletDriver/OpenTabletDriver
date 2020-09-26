using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Debugging
{
    public class DebugTabletReport : DebugDeviceReport, ITabletReport
    {
        public DebugTabletReport()
        {
        }

        public DebugTabletReport(ITabletReport tabletReport) : base(tabletReport)
        {
            this.ReportID = tabletReport.ReportID;
            this.Position = tabletReport.Position;
            this.Pressure = tabletReport.Pressure;
            this.PenButtons = tabletReport.PenButtons;
        }

        public uint ReportID { set; get; }
        public Vector2 Position { set; get; }
        public uint Pressure { set; get; }
        public bool[] PenButtons { set; get; }
    }
}