using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Debugging
{
    public class DebugAuxReport : DebugDeviceReport, IAuxReport
    {
        public DebugAuxReport()
        {
        }

        public DebugAuxReport(IAuxReport auxReport) : base(auxReport)
        {
            this.AuxButtons = auxReport.AuxButtons;
        }

        public bool[] AuxButtons { set; get; }
    }
}