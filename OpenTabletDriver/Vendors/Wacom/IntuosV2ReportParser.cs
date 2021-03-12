using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public class IntuosV2ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x2 => new IntuosV2TabletReport(data),
                0x10 => new IntuosV2TabletReport(data),
                0x3 => new IntuosV2AuxReport(data),
                _ => new DeviceReport(data)
            };
        }
    }
}