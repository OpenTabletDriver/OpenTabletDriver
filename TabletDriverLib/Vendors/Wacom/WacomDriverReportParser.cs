using System.Linq;
using TabletDriverLib.Tablet;

namespace TabletDriverLib.Vendors.Wacom
{
    public class WacomDriverReportParser : ITabletReportParser
    {
        public ITabletReport Parse(byte[] data)
        {
            return new WacomTabletReport(data);
        }
    }
}