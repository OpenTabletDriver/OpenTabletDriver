using System;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV1
{
    public class WacomDriverIntuosV1ReportParser : IntuosV1ReportParser
    {
        public override bool Parse(scoped in ReadOnlySpan<byte> report, scoped ref DeviceReport deviceReport)
        {
            return base.Parse(report[1..], ref deviceReport);
        }
    }
}
