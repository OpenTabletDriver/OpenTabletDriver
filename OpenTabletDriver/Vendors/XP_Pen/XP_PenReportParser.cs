using OpenTabletDriver.Tablet;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Vendors.XP_Pen
{
    public class XP_PenReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return (data[1] & 0xf0) == 0xf0 ? new XP_PenAuxReport(data) : new TabletReport(data);
        }
    }
}