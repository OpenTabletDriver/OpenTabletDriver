using System;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosPro
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class IntuosProReportParser : IntuosV1ReportParser
    {
        public override bool Parse(scoped in ReadOnlySpan<byte> report, scoped ref DeviceReport deviceReport)
        {
            return report[0] switch
            {
                0x03 => IntuosProAuxReport.Parse(report, ref deviceReport),
                _ => base.Parse(in report, ref deviceReport),
            };
        }
    }
}
