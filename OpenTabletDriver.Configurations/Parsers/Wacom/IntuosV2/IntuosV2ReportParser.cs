using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Parsers.Wacom.IntuosV2
{
    public class IntuosV2ReportParser : IReportParser<IDeviceReport>
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
                    0x10 => new IntuosV2Report(data),
                    0x11 => new IntuosV2AuxReport(data),
                    0xD2 => new IntuosV2TouchReport(data),
                    _ => new DeviceReport(data)
                };
            }
        }
    }
}