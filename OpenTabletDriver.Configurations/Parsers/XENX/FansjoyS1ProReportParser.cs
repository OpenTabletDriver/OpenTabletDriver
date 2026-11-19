using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.XENX
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class FansjoyS1ProReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x01 or 0x0a => report[1] != 0 ? new FansjoyS1ProTabletReport(report) : new OutOfRangeReport(report),
                0x02 => new FansjoyS1ProAuxReport(report),
                _ => new DeviceReport(report),
            };
        }
    }
}
