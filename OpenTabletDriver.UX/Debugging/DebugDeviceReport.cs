using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.UX.Debugging
{
    public class DebugDeviceReport : IDeviceReport
    {
        public byte[] Raw { set; get; }
    }
}