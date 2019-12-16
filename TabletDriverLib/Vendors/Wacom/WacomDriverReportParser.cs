using System.Linq;
using TabletDriverLib.Tablet;

namespace TabletDriverLib.Vendors.Wacom
{
    public class WacomDriverReportParser : IDeviceReportParser
    {
        public IDeviceReport Parse(byte[] data)
        {
            return new TabletReport(data.Skip(1).ToArray());
        }
    }
}