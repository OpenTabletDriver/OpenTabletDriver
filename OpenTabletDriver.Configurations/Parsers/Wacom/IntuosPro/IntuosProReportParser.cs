using OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosPro
{
    public class IntuosProReportParser : IntuosV1ReportParser
    {
        public override IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x02 => GetToolReport(report),
                0x10 => GetToolReport(report),
                0x03 => new IntuosProAuxReport(report, ref _lastWheelPosition),
                _ => new DeviceReport(report)
            };
        }

        private uint? _lastWheelPosition;
    }
}
