using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Intuos
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class WacomDriverIntuosReportParser : IntuosReportParser
    {
        public override IDeviceReport Parse(byte[] report)
        {
            return base.Parse(report[1..^0]);
        }
    }
}
