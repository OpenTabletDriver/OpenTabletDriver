using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Tablet
{
    public class TabletReportParser : IDeviceReportParser
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return new TabletReport(data);
        }
    }
}