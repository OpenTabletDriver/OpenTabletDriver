using OpenTabletDriver.Tablet;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.XP
{
    public class XPReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return (data[1] & 0xc0) == 0xc0 ? new XPAuxReport(data) : new TabletReport(data);
        }
    }
}