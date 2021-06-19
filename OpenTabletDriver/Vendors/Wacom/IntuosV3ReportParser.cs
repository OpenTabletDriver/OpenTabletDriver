using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Vendors.Wacom
{
    public class IntuosV3ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            if (data.Length < 10)
            {
                return new DeviceReport(data);
            }
            else
            {
                return data[0] switch
                {
                    0x10 => new IntuosV3Report(data),
                    0x11 => new IntuosV3AuxReport(data),
                    0xD2 => new IntuosV3TouchReport(data),
                    _ => new DeviceReport(data)
                };
            }
        }
    }
}