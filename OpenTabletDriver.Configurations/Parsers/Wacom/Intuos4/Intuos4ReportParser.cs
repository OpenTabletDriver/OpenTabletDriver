using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos4
{
    public class Intuos4ReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {
            return data[0] switch
            {
                0x02 => GetToolReport(data),
                0x10 => _IntuosV1ReportParser.Parse(data),
                0x0C => new Intuos4AuxReport(data),
                _ => new DeviceReport(data)
            };
        }

        private IDeviceReport GetToolReport(byte[] data)
        {
            return data[1] switch
            {
                0xEC => new Intuos4MouseReport(data),
                0xAC => new Intuos4MouseReport(data),
                _ => _IntuosV1ReportParser.Parse(data)
            };
        }

        private IntuosV1ReportParser _IntuosV1ReportParser = new IntuosV1ReportParser();
    }
}
