using TabletDriverPlugin.Tablet;

namespace OpenTabletDriverUX.Debugging
{
    public class DebugAuxReport : DebugDeviceReport, IAuxReport
    {
        public bool[] AuxButtons { set; get; }
    }
}