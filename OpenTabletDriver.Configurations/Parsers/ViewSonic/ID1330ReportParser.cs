using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ViewSonic
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class ID1330ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            if (report.Length == 0)
                return new DeviceReport(report);

            return report[0] switch
            {
                0x02 when report.Length >= 14 && (report[9] & 0b11) != 0b11 => new OutOfRangeReport(report),
                0x02 when report.Length >= 14 => new ID1330VendorReport(report),
                0x10 when report.Length >= 13 && !report[1].IsBitSet(5) => new OutOfRangeReport(report),
                0x10 when report.Length >= 13 => new ID1330TabletReport(report),
                0xAC when report.Length >= 2 => new ID1330AuxReport(report),
                _ => new DeviceReport(report)
            };
        }
    }
}
