using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV3
{
    public class IntuosV3ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x1F => new IntuosV3Report(data),
                _ => new DeviceReport(data)
            };
        }
    }
}
