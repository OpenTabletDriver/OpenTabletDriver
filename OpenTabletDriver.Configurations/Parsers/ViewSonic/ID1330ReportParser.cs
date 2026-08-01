using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.ViewSonic
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class ID1330ReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x10 when !report[1].IsBitSet(5) => new OutOfRangeReport(report),
                0x10 => new ID1330TabletReport(report),
                0xAC => new ID1330AuxReport(report),
                _ => new DeviceReport(report)
            };
        }
    }
}
