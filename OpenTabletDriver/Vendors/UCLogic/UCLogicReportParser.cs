using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.UCLogic
{
    public class UCLogicReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            return (data[1] & 0xc0) == 0xc0 ? new UCLogicAuxReport(data) : new TabletReport(data);
        }
    }
}