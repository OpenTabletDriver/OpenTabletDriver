using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.Wacom.CintiqV1;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.Cintiq24HD
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class Cintiq24HDReportParser : CintiqV1ReportParser
    {
        public override IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x0C => new Cintiq24HDAuxReport(report),
                _ => base.Parse(report)
            };
        }
    }
}
