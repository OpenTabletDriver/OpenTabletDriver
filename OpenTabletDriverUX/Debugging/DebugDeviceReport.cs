using TabletDriverPlugin.Tablet;

namespace OpenTabletDriverUX.Debugging
{
    public class DebugDeviceReport : IDeviceReport
    {
        public byte[] Raw { set; get; }
    }
}